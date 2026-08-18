using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
/// </summary>
public class AuthService
{
    private readonly UserRepository _users;
    private readonly IHttpContextAccessor _http;

    public AuthService(UserRepository users, IHttpContextAccessor http)
    {
        _users = users;
        _http = http;
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
    public async Task<(LoginResult Result, AppUser? User)> LoginAsync(
        string email, string password, bool rememberMe)
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

        await SignInAsync(user, rememberMe);
        return (LoginResult.Success, user);
    }

    /// <summary>
    /// Writes the login cookie. The claims below are what every page can read
    /// afterwards, which is why the role goes in: it is what the [Authorize]
    /// attribute checks, without another trip to the database on every request.
    /// </summary>
    private async Task SignInAsync(AppUser user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name,           user.FullName),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           user.RoleName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            // Remember me keeps the cookie after the browser closes. Without it
            // the login lasts only as long as the browser session.
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await _http.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    /// <summary>Signs the user out and clears the cookie.</summary>
    public async Task LogoutAsync()
        => await _http.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    /// <summary>
    /// Registers a client from the public sign up page.
    ///
    /// The role is forced to Client here and is not a parameter, so that no
    /// amount of tampering with the form can create an administrator. Staff
    /// accounts are made by an Administrator on the Users and Access screen.
    /// </summary>
    public int RegisterClient(AppUser user, string password)
    {
        user.RoleId = ClientRoleId;
        return _users.Create(user, password);
    }

    /// <summary>
    /// The Client role id. It is 4 because of the order the roles are inserted
    /// in 01-schema.sql: Administrator, Supervisor, Travel Agent, Client.
    /// </summary>
    public const int ClientRoleId = 4;

    /// <summary>
    /// The signed in user's id, or null when nobody is signed in.
    /// Pages use this to load "my trips" without trusting anything the browser
    /// sent them.
    /// </summary>
    public int? CurrentUserId
    {
        get
        {
            var value = _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    /// <summary>The signed in user's role name, or null.</summary>
    public string? CurrentRole
        => _http.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
}
