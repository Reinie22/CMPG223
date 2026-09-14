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

    private const int SingleLineLimit = 200;
    private const int MultiLineLimit  = 4000;

    protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
    {
        if (Context.Handler is System.Web.UI.Page page)
            page.PreLoad += (s, args) => LimitTextLength(page);
    }

    private static void LimitTextLength(System.Web.UI.Control parent)
    {
        foreach (System.Web.UI.Control child in parent.Controls)
        {
            if (child is System.Web.UI.WebControls.TextBox box)
            {
                var limit = box.MaxLength > 0
                    ? box.MaxLength
                    : box.TextMode == System.Web.UI.WebControls.TextBoxMode.MultiLine ? MultiLineLimit : SingleLineLimit;

                if (box.Text.Length > limit)
                    box.Text = box.Text.Substring(0, limit);
            }

            if (child.HasControls())
                LimitTextLength(child);
        }
    }

    private static readonly object _errorLogLock = new object();

    protected void Application_Error(object sender, EventArgs e)
    {
        try
        {
            var error = Server.GetLastError();

            if (error == null)
                return;

            if (error is HttpException http && http.GetHttpCode() == 404)
                return;

            var folder = Server.MapPath("~/App_Data/Logs");
            System.IO.Directory.CreateDirectory(folder);

            var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {Request.HttpMethod} {Request.RawUrl}"
                        + Environment.NewLine + error.GetBaseException()
                        + Environment.NewLine + new string('-', 80) + Environment.NewLine;

            lock (_errorLogLock)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(folder, "errors.log"), entry);
            }
        }
        catch (Exception)
        {
        }
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

            _testUserLoaded = _testUser != null;
        }

        return _testUser;
    }
}
