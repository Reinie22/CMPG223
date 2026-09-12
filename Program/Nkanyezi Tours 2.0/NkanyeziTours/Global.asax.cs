using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace NkanyeziTours;

/// <summary>
/// Application start-up and per-request events.
///
/// This is the Web Forms equivalent of Program.cs, and there is far less in it,
/// because authentication and the role rules are configured in Web.config
/// rather than in code. The one thing that MUST happen in code is below.
/// </summary>
public class Global : HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        RegisterJQueryForValidators();
    }

    /// <summary>
    /// Tells Web Forms where to find jQuery, so the validator controls work.
    ///
    /// WITHOUT THIS, EVERY PAGE THAT HAS A VALIDATOR ON IT RETURNS A BLANK 500.
    ///
    /// Since .NET 4.5, RequiredFieldValidator, RangeValidator and friends render
    /// their client-side checking in "unobtrusive" mode, which is built on
    /// jQuery. If nothing has told ASP.NET where jQuery is, the first page
    /// containing a validator throws:
    ///
    ///     WebForms UnobtrusiveValidationMode requires a ScriptResourceMapping
    ///     for 'jquery'. Please add a ScriptResourceMapping named jquery
    ///     (case-sensitive).
    ///
    /// It is a horrible error to diagnose, because pages WITHOUT validators keep
    /// working perfectly - so it looks like one specific page is broken rather
    /// than a missing application-wide registration. Expect to hit it the moment
    /// you add your first validator to your own page; it is already fixed here.
    ///
    /// The other way to solve it is to switch unobtrusive validation off in
    /// Web.config with ValidationSettings:UnobtrusiveValidationMode = None. We
    /// register jQuery instead, because switching it off makes every page
    /// render a block of inline JavaScript rather than using the one shared
    /// script file.
    ///
    /// The name "jquery" is case-sensitive. "jQuery" does not work.
    /// </summary>
    private static void RegisterJQueryForValidators()
    {
        System.Web.UI.ScriptManager.ScriptResourceMapping.AddDefinition(
            "jquery",
            new System.Web.UI.ScriptResourceDefinition
            {
                Path      = "~/Scripts/jquery.min.js",
                DebugPath = "~/Scripts/jquery.min.js"
            });
    }

    /// <summary>
    /// Puts the user's role onto the current request, on every request.
    ///
    /// WHY THIS METHOD IS NOT OPTIONAL
    /// ===============================
    /// AuthService packs the role name into the login ticket when somebody
    /// signs in. But ASP.NET does not look inside that field by itself - by
    /// default the signed-in user arrives at each request with a name and no
    /// roles at all.
    ///
    /// Web.config protects whole folders with rules like:
    ///
    ///     &lt;allow roles="Administrator, Supervisor, Travel Agent" /&gt;
    ///     &lt;deny users="*" /&gt;
    ///
    /// Those are checked against the roles on the current principal. With no
    /// roles attached, the allow line never matches, the deny line wins, and
    /// every signed-in user is thrown out of every protected folder. The login
    /// itself works perfectly, which is what makes it confusing: you sign in
    /// successfully and are immediately bounced back to the login page.
    ///
    /// This method unpacks the role from the ticket and rebuilds the principal
    /// with it, which is what makes the Web.config rules work.
    ///
    /// It runs on every single request, so keep it cheap - no database calls.
    /// </summary>
    protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
    {
        var context = HttpContext.Current;

        if (context?.User?.Identity is not FormsIdentity identity || !identity.IsAuthenticated)
        {
            // Nobody is signed in. Normally that is the end of it and the
            // Web.config rules bounce them to the login page. While the testing
            // bypass is on, hand them an Administrator instead - see
            // SignInTestUser below.
            if (context != null && AuthService.BypassLogin)
                SignInTestUser(context);

            return;
        }

        // UserData was packed by AuthService as "RoleName|Full Name".
        // Only the first half is the role.
        var role = identity.Ticket?.UserData?.Split('|').FirstOrDefault();

        if (string.IsNullOrWhiteSpace(role))
            return;

        // GenericPrincipal is the plain "an identity plus some roles" object.
        // Handing it back to the context is what User.IsInRole() and the
        // Web.config rules both read from.
        context.User = new GenericPrincipal(identity, new[] { role });
    }

    // -------------------------------------------------------------------------
    // The testing bypass
    // -------------------------------------------------------------------------

    /// <summary>The Administrator the bypass signs in as. Looked up once.</summary>
    private static AppUser? _testUser;
    private static bool _testUserLoaded;
    private static readonly object _testUserLock = new object();

    /// <summary>
    /// Makes the current request look exactly as though an Administrator had
    /// signed in a moment ago. Only ever called while BypassLogin is on.
    ///
    /// WHY BUILD A REAL TICKET RATHER THAN A PLAIN GenericIdentity
    /// ==========================================================
    /// The pages do not all ask the same question. Web.config asks for a role,
    /// AuthService.CurrentUserId reads the identity's Name, and
    /// CurrentDisplayName and CurrentRole both dig into the ticket's UserData.
    /// A plain GenericIdentity has no ticket, so the navigation bar would come
    /// out blank and CreatedBy would be null on every booking.
    ///
    /// Building the same FormsAuthenticationTicket that a genuine login builds
    /// means every one of those reads finds what it expects, and no page needs
    /// a special case for the bypass.
    ///
    /// Nothing is written to a cookie. This lasts one request and is rebuilt on
    /// the next, which is what makes a real login able to take over instantly:
    /// once a genuine cookie exists, the branch above runs and this is skipped.
    /// </summary>
    private static void SignInTestUser(HttpContext context)
    {
        var user = TestUser();

        if (user == null)
            return;

        var ticket = new FormsAuthenticationTicket(
            version:     1,
            name:        user.UserId.ToString(CultureInfo.InvariantCulture),
            issueDate:   DateTime.Now,
            expiration:  DateTime.Now.AddHours(8),
            isPersistent: false,
            userData:    user.RoleName + "|" + user.FullName,
            cookiePath:  FormsAuthentication.FormsCookiePath);

        context.User = new GenericPrincipal(
            new FormsIdentity(ticket),
            new[] { user.RoleName });
    }

    /// <summary>
    /// The first Administrator in the database, fetched once and then kept.
    ///
    /// The id has to be a real one, because bookings are saved with
    /// CreatedBy = AuthService.CurrentUserId and the foreign key is checked by
    /// SQL Server. A made-up number would fail on save.
    ///
    /// PostAuthenticateRequest runs on every single request, so this must not
    /// become a database call per request - hence the cache and the lock. A null
    /// result is cached too: if there is no Administrator seeded yet, we should
    /// ask once and then stop asking.
    /// </summary>
    private static AppUser? TestUser()
    {
        if (_testUserLoaded)
            return _testUser;

        lock (_testUserLock)
        {
            if (_testUserLoaded)
                return _testUser;

            _testUser = Repositories.Users
                                    .GetByRole(RoleNames.Administrator)
                                    .FirstOrDefault();
            _testUserLoaded = true;
        }

        return _testUser;
    }
}
