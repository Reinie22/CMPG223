using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Account;

public partial class Register : Page
{
    private UserRepository _users => Repositories.Users;
    private AuthService _auth => Repositories.Auth;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && AuthService.IsSignedIn && !AuthService.BypassLogin)
            Response.Redirect("~/Default.aspx");
    }

    protected void btnSignUp_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var name     = txtName.Text.Trim();
        var surname  = txtSurname.Text.Trim();
        var email    = txtEmail.Text.Trim();
        var phone    = txtPhone.Text.Trim();
        var password = txtPassword.Text;

        if (!IsComplete(name, surname, email, password))
            return;

        if (_users.EmailExists(email))
        {
            ShowError("There is already an account with that email address. "
                    + "Try signing in instead, or use another address.");
            return;
        }

        var client = new AppUser
        {
            Name          = name,
            Surname       = surname,
            Email         = email,
            ContactNumber = phone
        };

        try
        {
            _users.RegisterClient(client, password);
        }
        catch (SqlException ex) when (IsDuplicateEmail(ex))
        {
            ShowError("There is already an account with that email address. "
                    + "Try signing in instead, or use another address.");
            return;
        }
        catch (Exception)
        {
            ShowError("That account could not be created. Please try again.");
            return;
        }

        var (result, _) = _auth.Login(email, password, rememberMe: false);

        if (result == AuthService.LoginResult.Success)
            Response.Redirect("~/Default.aspx?welcome=1");

        Response.Redirect("~/Account/Login.aspx?registered=1");
    }

    private bool IsComplete(string name, string surname, string email, string password)
    {
        if (name.Length == 0 || surname.Length == 0 || email.Length == 0)
        {
            ShowError("Please fill in your name, surname and email address.");
            return false;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowError("That email address does not look right. "
                    + "It should look like name@example.com.");
            return false;
        }

        if (password.Length < 8)
        {
            ShowError("Your password needs to be at least 8 characters long.");
            return false;
        }

        if (password != txtConfirm.Text)
        {
            ShowError("The two passwords do not match.");
            return false;
        }

        return true;
    }

    private static bool IsDuplicateEmail(SqlException ex)
        => ex.Number == 2627 || ex.Number == 2601;

    private void ShowError(string message)
    {
        litError.Text = Server.HtmlEncode(message);
        pnlError.Visible = true;
        txtPassword.Text = "";
        txtConfirm.Text  = "";
    }
}
