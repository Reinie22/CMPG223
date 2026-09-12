using System.Configuration;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace Nkanyezi.Services;

/// <summary>
/// The rules around logging in, as opposed to the data.
///
/// This is a SERVICE. Services hold the decisions: is this login valid, is this
/// account allowed in, what should happen next. Repositories only fetch and
/// save rows. Keeping the two apart means the rule is written once, and a page
/// cannot accidentally implement it slightly differently.
///
/// WHAT CHANGED FROM THE OLD PROJECT
/// =================================
/// The ASP.NET Core version signed people in with cookie authentication
/// middleware and an async SignInAsync call. Web Forms uses FormsAuthentication
/// instead, which is configured in Web.config, and the whole thing is
/// synchronous - Web Forms needs extra plumbing to await anything in a page, and
/// there is nothing here worth that trouble.
///
/// The one part worth understanding is how the ROLE travels, because the
/// &lt;allow roles="..."&gt; rules in Web.config do not work without it. See
/// IssueTicket below.
/// </summary>
public class AuthService
{
    private readonly UserRepository _users;

    public AuthService(UserRepository users)
    {
        _users = users;
    }

    /// <summary>
    /// The result of trying to log in. An enum rather than a bool, because
    /// "wrong password" and "account deactivated" need different messages.
    /// </summary>
    public enum LoginResult
    {
        Success,
        InvalidCredentials,
        AccountDeactivated
    }

    /// <summary>
    /// Checks the email and password and, when they are right, signs the user
    /// in by issuing the cookie the browser sends back on every later request.
    /// </summary>
    public (LoginResult Result, AppUser? User) Login(string email, string password, bool rememberMe)
    {
        // The only call in the system that returns the password hash.
        var user = _users.GetByEmailForLogin(email);

        // Deliberately the same answer for "no such account" and "wrong
        // password". Telling somebody which one it was lets them work out which
        // email addresses have accounts on the system.
        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            return (LoginResult.InvalidCredentials, null);

        // Checked only after the password, for the same reason.
        if (!user.IsActive)
            return (LoginResult.AccountDeactivated, null);

        IssueTicket(user, rememberMe);
        return (LoginResult.Success, user);
    }

    /// <summary>
    /// Writes the login cookie.
    ///
    /// READ THIS PART. It is the one piece of Web Forms authentication that is
    /// not obvious, and getting it wrong locks everybody out of everything.
    ///
    /// FormsAuthentication normally stores only the user name in its cookie. It
    /// knows nothing about roles. But Web.config protects whole folders with
    /// rules like:
    ///
    ///     &lt;allow roles="Administrator, Supervisor, Travel Agent" /&gt;
    ///
    /// Those rules are checked against the roles on the current user's
    /// principal object. If nothing ever puts the role there, every one of those
    /// rules fails to match, the following &lt;deny users="*" /&gt; wins, and
    /// every signed-in user is bounced out of every protected folder - which
    /// looks exactly like "the login is broken".
    ///
    /// So the role is packed into the ticket's UserData field here, and
    /// unpacked again on every request in Global.asax.cs. The two halves must
    /// agree, so if you change one, change the other.
    ///
    /// The ticket is encrypted, so a user cannot open the cookie and promote
    /// themselves to Administrator by editing the text.
    /// </summary>
    private static void IssueTicket(AppUser user, bool rememberMe)
    {
        var ticket = new FormsAuthenticationTicket(
            version:     1,
            name:        user.UserId.ToString(),          // who they are
            issueDate:   DateTime.Now,
            expiration:  DateTime.Now.AddHours(rememberMe ? 24 * 14 : 8),
            isPersistent: rememberMe,
            // Role first, then the person's name, separated by a pipe. The role
            // rules depend on the first half; the navigation bar uses the second
            // so it can greet somebody by name without a database call on every
            // single request. Unpacked by CurrentRole and CurrentDisplayName.
            userData:    user.RoleName + "|" + user.FullName,
            cookiePath:  FormsAuthentication.FormsCookiePath);

        var encrypted = FormsAuthentication.Encrypt(ticket);

        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
        {
            HttpOnly = true,                              // JavaScript cannot read it
            Path     = FormsAuthentication.FormsCookiePath
        };

        // A persistent cookie survives the browser closing. A non-persistent one
        // disappears when it closes, which is what you want on a shared machine.
        if (rememberMe)
            cookie.Expires = ticket.Expiration;

        HttpContext.Current.Response.Cookies.Add(cookie);
    }

    /// <summary>Ends the session and clears the cookie.</summary>
    public static void Logout()
    {
        FormsAuthentication.SignOut();
        HttpContext.Current.Session?.Abandon();
    }

    // -------------------------------------------------------------------------
    // Reading the signed-in user, for the pages
    // -------------------------------------------------------------------------

    /// <summary>
    /// The signed-in user's id, or null when nobody is signed in.
    ///
    /// The id is stored in the ticket rather than looked up from the database on
    /// every request, which keeps it to one cookie read.
    /// </summary>
    public static int? CurrentUserId
    {
        get
        {
            var user = HttpContext.Current?.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return null;

            return int.TryParse(user.Identity.Name, out var id) ? id : null;
        }
    }

    /// <summary>The signed-in user's role name, or an empty string.</summary>
    public static string CurrentRole => TicketPart(0);

    /// <summary>
    /// The signed-in user's full name, for the navigation bar.
    /// Comes out of the login ticket, so it costs nothing to read.
    /// </summary>
    public static string CurrentDisplayName => TicketPart(1);

    /// <summary>
    /// Pulls one half out of the ticket's UserData, which AuthService packed as
    /// "RoleName|Full Name" when the user signed in.
    /// </summary>
    private static string TicketPart(int index)
    {
        var identity = HttpContext.Current?.User?.Identity as FormsIdentity;
        var data = identity?.Ticket?.UserData;

        if (string.IsNullOrEmpty(data))
            return "";

        // data! because on .NET Framework string.IsNullOrEmpty is not annotated
        // with [NotNullWhen(false)], so the compiler cannot see that the check
        // above already ruled null out. On .NET 10 this was not needed.
        var parts = data!.Split('|');
        return index < parts.Length ? parts[index] : "";
    }

    public static bool IsSignedIn => CurrentUserId != null;

    // -------------------------------------------------------------------------
    // The testing bypass
    // -------------------------------------------------------------------------

    /// <summary>
    /// True while BypassLogin is "true" in Web.config's appSettings.
    ///
    /// WHAT IT IS FOR
    /// ==============
    /// The screens are being built by several people at once, and most of them
    /// are behind a role rule in Web.config. While the login and the user table
    /// are still being finished, that means nobody can open anybody else's page
    /// to check it. This switch lets the whole system be clicked through with no
    /// sign-in at all.
    ///
    /// WHAT IT DOES NOT DO
    /// ===================
    /// It does not delete or weaken anything. The login screen, the password
    /// hashing, the ticket and every rule in Web.config are all exactly as they
    /// were. Global.asax simply hands each request a ready-made Administrator
    /// when nobody is signed in, so the rules pass instead of failing.
    ///
    /// Set it back to "false" in Web.config and everything enforces again, with
    /// no code to undo.
    /// </summary>
    public static bool BypassLogin =>
        string.Equals(
            ConfigurationManager.AppSettings["BypassLogin"],
            "true",
            StringComparison.OrdinalIgnoreCase);

    public static bool IsInRole(string roleName)
        => HttpContext.Current?.User?.IsInRole(roleName) ?? false;

    /// <summary>True for anyone who works for the agency, as opposed to a client.</summary>
    public static bool IsStaff => IsSignedIn && CurrentRole != RoleNames.Client;

    /// <summary>
    /// The full record of the signed-in user, fetched from the database.
    ///
    /// Use the properties above where you only need the id or the role. This
    /// costs a database call, so do not put it in a loop.
    /// </summary>
    public AppUser? GetCurrentUser()
    {
        var id = CurrentUserId;
        return id == null ? null : _users.GetById(id.Value);
    }
}
