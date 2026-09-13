using System.Configuration;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace Nkanyezi.Services;

public class AuthService
{
    private readonly UserRepository _users;

    public AuthService(UserRepository users)
    {
        _users = users;
    }

    public enum LoginResult
    {
        Success,
        InvalidCredentials,
        AccountDeactivated
    }

    public (LoginResult Result, AppUser? User) Login(string email, string password, bool rememberMe)
    {
        var user = _users.GetByEmailForLogin(email);

        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            return (LoginResult.InvalidCredentials, null);

        if (!user.IsActive)
            return (LoginResult.AccountDeactivated, null);

        IssueTicket(user, rememberMe);
        return (LoginResult.Success, user);
    }

    private static void IssueTicket(AppUser user, bool rememberMe)
    {
        var ticket = new FormsAuthenticationTicket(
            version:     1,
            name:        user.UserId.ToString(),
            issueDate:   DateTime.Now,
            expiration:  DateTime.Now.AddHours(rememberMe ? 24 * 14 : 8),
            isPersistent: rememberMe,

            userData:    user.RoleName + "|" + user.FullName,
            cookiePath:  FormsAuthentication.FormsCookiePath);

        var encrypted = FormsAuthentication.Encrypt(ticket);

        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
        {
            HttpOnly = true,
            Path     = FormsAuthentication.FormsCookiePath
        };

        if (rememberMe)
            cookie.Expires = ticket.Expiration;

        HttpContext.Current.Response.Cookies.Add(cookie);
    }

    public static void Logout()
    {
        FormsAuthentication.SignOut();
        HttpContext.Current.Session?.Abandon();
    }

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

    public static string CurrentRole => TicketPart(0);

    public static string CurrentDisplayName => TicketPart(1);

    private static string TicketPart(int index)
    {
        var identity = HttpContext.Current?.User?.Identity as FormsIdentity;
        var data = identity?.Ticket?.UserData;

        if (string.IsNullOrEmpty(data))
            return "";

        var parts = data!.Split('|');
        return index < parts.Length ? parts[index] : "";
    }

    public static bool IsSignedIn => CurrentUserId != null;

    public static bool BypassLogin =>
        string.Equals(
            ConfigurationManager.AppSettings["BypassLogin"],
            "true",
            StringComparison.OrdinalIgnoreCase);

    public static bool IsInRole(string roleName)
        => HttpContext.Current?.User?.IsInRole(roleName) ?? false;

    public static bool IsStaff => IsSignedIn && CurrentRole != RoleNames.Client;

    public AppUser? GetCurrentUser()
    {
        var id = CurrentUserId;
        return id == null ? null : _users.GetById(id.Value);
    }
}
