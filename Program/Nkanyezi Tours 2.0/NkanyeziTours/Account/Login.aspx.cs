using System.Web.Security;
using System.Web.UI.WebControls;
using Nkanyezi.Models;
using System.Web.UI;

namespace NkanyeziTours.Account;

/// <summary>
/// The login screen.
///
/// Notice how little is here. The page reads the two boxes, hands them to
/// AuthService, and reacts to the answer. It does not know what a password hash
/// is, it does not touch the database, and there is no SQL anywhere in sight.
/// That is the layering working.
/// </summary>
public partial class Login : Page
{
    private readonly AuthService _auth = Repositories.Auth;

    protected void Page_Load(object sender, EventArgs e)
    {
        // Somebody already signed in has no business on the login page.
        //
        // The exception is the testing bypass: it makes EVERY visitor look
        // signed in, so without this the login screen could never be opened at
        // all. Leaving it reachable means a real login can still be tested, and
        // a real login then takes over from the bypass.
        if (!IsPostBack && AuthService.IsSignedIn && !AuthService.BypassLogin)
            Response.Redirect("~/Default.aspx");

        // The test-account list is a development aid, so it lives and dies
        // with the same Web.config switch as the bypass.
        pnlTestAccounts.Visible = AuthService.BypassLogin;

        if (AuthService.BypassLogin && !IsPostBack)
            BindTestAccounts();
    }

    protected void btnSignIn_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var (result, user) = _auth.Login(txtEmail.Text.Trim(), txtPassword.Text, chkRemember.Checked);

        switch (result)
        {
            case AuthService.LoginResult.Success:
                // ReturnUrl is added by forms authentication when somebody is
                // bounced here from a page they were not signed in for. Sending
                // them back to it means they carry on where they left off.
                //
                // FormsAuthentication.RedirectFromLoginPage is deliberately NOT
                // used, because it would write a second cookie without the role
                // in it and quietly undo the role setup in AuthService.
                var returnUrl = Request.QueryString["ReturnUrl"];

                if (!string.IsNullOrWhiteSpace(returnUrl) && IsLocalUrl(returnUrl))
                    Response.Redirect(returnUrl);
                else
                    Response.Redirect(FormsAuthentication.DefaultUrl);
                break;

            case AuthService.LoginResult.AccountDeactivated:
                ShowError("This account has been deactivated. Speak to an administrator.");
                break;

            default:
                ShowError("That email address and password do not match an account.");
                break;
        }
    }

    /// <summary>One row of the test-account list.</summary>
    private sealed class TestAccount
    {
        public string RoleName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleCssClass { get; set; } = "";
    }

    /// <summary>
    /// Every active account, grouped by role so the four roles are easy to
    /// tell apart while testing.
    /// </summary>
    private void BindTestAccounts()
    {
        var users = Repositories.Users.GetAll();

        rptTestAccounts.DataSource = users
            .OrderBy(u => u.RoleId)
            .ThenBy(u => u.Surname)
            .Select(u => new TestAccount
            {
                RoleName = u.RoleName,
                FullName = u.FullName,
                Email    = u.Email,
                RoleCssClass = u.RoleName switch
                {
                    RoleNames.Administrator => "text-bg-danger",
                    RoleNames.Supervisor    => "text-bg-warning",
                    RoleNames.TravelAgent   => "text-bg-primary",
                    _                       => "text-bg-secondary"
                }
            })
            .ToList();

        rptTestAccounts.DataBind();
    }

    /// <summary>
    /// Clicking an email fills the form in rather than signing straight in,
    /// so the real login path is still the thing being exercised.
    /// </summary>
    protected void rptTestAccounts_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "UseAccount")
            return;

        txtEmail.Text = Convert.ToString(e.CommandArgument) ?? "";
        txtPassword.Text = "Password123";
    }

    /// <summary>
    /// Only ever redirect to a page on THIS site.
    ///
    /// Without this check, somebody could email a link like
    /// Login.aspx?ReturnUrl=http://not-our-site.example and the user would be
    /// sent there straight after a successful login, having just typed their
    /// password. It is called an open redirect and it is used for phishing.
    /// </summary>
    private static bool IsLocalUrl(string url)
        => url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");

    private void ShowError(string message)
    {
        litError.Text  = Server.HtmlEncode(message);
        pnlError.Visible = true;
        txtPassword.Text = "";
    }
}
