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
            return;

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
}
