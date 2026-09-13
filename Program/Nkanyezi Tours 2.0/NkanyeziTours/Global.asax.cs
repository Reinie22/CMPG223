using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace NkanyeziTours;

public class Global : HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        RegisterJQueryForValidators();
    }

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

    protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
    {
        var context = HttpContext.Current;

        if (context?.User?.Identity is not FormsIdentity identity || !identity.IsAuthenticated)
        {
            if (context != null && AuthService.BypassLogin)
                SignInTestUser(context);

            return;
        }

        var role = identity.Ticket?.UserData?.Split('|').FirstOrDefault();

        if (string.IsNullOrWhiteSpace(role))
            return;

        context.User = new GenericPrincipal(identity, new[] { role });
    }

    private static AppUser? _testUser;
    private static bool _testUserLoaded;
    private static readonly object _testUserLock = new object();

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

    private static AppUser? TestUser()
    {
        if (_testUserLoaded)
            return _testUser;

        lock (_testUserLock)
        {
            if (_testUserLoaded)
                return _testUser;

            try
            {
                _testUser = Repositories.Users
                                        .GetByRole(RoleNames.Administrator)
                                        .FirstOrDefault();
            }
            catch (Exception)
            {
                _testUser = null;
            }

            _testUserLoaded = true;
        }

        return _testUser;
    }
}
