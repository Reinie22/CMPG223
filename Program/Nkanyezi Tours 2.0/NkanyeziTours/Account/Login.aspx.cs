using System.Web.Security;
using System.Web.UI.WebControls;
using Nkanyezi.Models;
using System.Web.UI;

namespace NkanyeziTours.Account;

public partial class Login : Page
{
    private AuthService _auth => Repositories.Auth;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && AuthService.IsSignedIn && !AuthService.BypassLogin)
            Response.Redirect("~/Default.aspx");

        pnlTestAccounts.Visible = AuthService.BypassLogin;

        if (AuthService.BypassLogin && !IsPostBack)
            BindTestAccounts();
    }

    protected void btnSignIn_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var email    = txtEmail.Text.Trim();
        var password = txtPassword.Text;

        if (email.Length == 0 || password.Length == 0)
        {
            ShowError("Enter your email address and your password.");
            return;
        }

        var (result, user) = _auth.Login(email, password, chkRemember.Checked);

        switch (result)
        {
            case AuthService.LoginResult.Success:

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

    private sealed class TestAccount
    {
        public string RoleName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleCssClass { get; set; } = "";
    }

    private void BindTestAccounts()
    {
        List<AppUser> users;

        try
        {
            users = Repositories.Users.GetAll();
        }
        catch (Exception ex)
        {
            pnlTestAccounts.Visible = false;
            ShowError(RealMessage(ex));
            return;
        }

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

    protected void rptTestAccounts_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "UseAccount")
            return;

        txtEmail.Text = Convert.ToString(e.CommandArgument) ?? "";
        txtPassword.Text = "Password123";
    }

    private static bool IsLocalUrl(string url)
        => url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");

    private void ShowError(string message)
    {
        litError.Text  = Server.HtmlEncode(message);
        pnlError.Visible = true;
        txtPassword.Text = "";
    }

    private static string RealMessage(Exception ex)
    {
        while (ex is TypeInitializationException && ex.InnerException != null)
            ex = ex.InnerException;

        return ex.Message;
    }
}
