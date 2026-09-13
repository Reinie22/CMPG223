using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Users;

public partial class Edit : Page
{
    private UserRepository _users => Repositories.Users;
    private LookupRepository _lookups => Repositories.Lookups;

    private int? UserId =>
        int.TryParse(Request.QueryString["id"], out var id) ? id : (int?)null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
            return;

        BindRoles();

        if (UserId == null)
        {
            litPasswordHelp.Text = "They will use this to sign in.";
            return;
        }

        var user = _users.GetById(UserId.Value);

        if (user == null)
        {
            Response.Redirect("~/Maintain/Users/Default.aspx");
            return;
        }

        litHeading.Text       = "Edit " + Server.HtmlEncode(user.FullName);
        txtName.Text          = user.Name;
        txtSurname.Text       = user.Surname;
        txtEmail.Text         = user.Email;
        txtPhone.Text         = user.ContactNumber;
        chkActive.Checked     = user.IsActive;
        ddlRole.SelectedValue = user.RoleId.ToString();

        litPasswordLabel.Text  = "New password";
        litPasswordHelp.Text   = "Leave blank to keep the current password.";
        rfvPassword.Enabled    = false;
    }

    private void BindRoles()
    {
        ddlRole.DataSource = _lookups.GetRoles();
        ddlRole.DataTextField = "Name";
        ddlRole.DataValueField = "Id";
        ddlRole.DataBind();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var email = txtEmail.Text.Trim();

        if (_users.EmailExists(email, UserId))
        {
            ShowMessage("Another account already uses that email address.", "alert-danger");
            return;
        }

        var user = new AppUser
        {
            Name          = txtName.Text.Trim(),
            Surname       = txtSurname.Text.Trim(),
            Email         = email,
            ContactNumber = txtPhone.Text.Trim(),
            RoleId        = int.Parse(ddlRole.SelectedValue),
            IsActive      = chkActive.Checked
        };

        var password = txtPassword.Text;

        if (!UserId.HasValue && string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("Give the new account a password of at least 8 characters.",
                        "alert-danger");
            return;
        }

        try
        {
            if (UserId.HasValue)
            {
                user.UserId = UserId.Value;
                _users.Update(user);

                if (!string.IsNullOrWhiteSpace(password))
                    _users.SetPassword(user.UserId, password);
            }
            else
            {
                _users.Create(user, password);
            }
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            ShowMessage("Another account already uses that email address.", "alert-danger");
            return;
        }
        catch (Exception ex)
        {
            ShowMessage("Could not save the account. " + Server.HtmlEncode(ex.Message),
                        "alert-danger");
            return;
        }

        Response.Redirect("~/Maintain/Users/Default.aspx?saved=1");
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
