using System.Web.Security;
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
