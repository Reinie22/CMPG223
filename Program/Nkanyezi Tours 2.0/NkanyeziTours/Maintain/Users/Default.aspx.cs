using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;

namespace NkanyeziTours.Maintain.Users;

public partial class _Default : Page
{
    private UserRepository _users => Repositories.Users;
    private LookupRepository _lookups => Repositories.Lookups;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        if (Request.QueryString["saved"] == "1")
            ShowMessage("User saved.", "alert-success");

        BindRoles();
        BindGrid();
    }

    private void BindRoles()
    {
        ddlRole.DataSource = _lookups.GetRoles();
        ddlRole.DataTextField = "Name";
        ddlRole.DataValueField = "Id";
        ddlRole.DataBind();
        ddlRole.Items.Insert(0, new ListItem("Every role", ""));
    }

    private void BindGrid()
    {
        var text = txtSearchText.Text.Trim();
        int? roleId = int.TryParse(ddlRole.SelectedValue, out var id) ? id : (int?)null;

        var hasFilter = text.Length > 0 || roleId.HasValue;

        gvUsers.DataSource = hasFilter
            ? _users.Search(text, roleId, includeInactive: true)
            : _users.GetAll(includeInactive: true);

        gvUsers.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e) => BindGrid();

    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtSearchText.Text = "";
        ddlRole.SelectedIndex = 0;
        BindGrid();
    }

    protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        var userId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "DeactivateUser")
        {
            _users.SetActive(userId, false);
            ShowMessage("That account can no longer sign in.", "alert-success");
            BindGrid();
        }
        else if (e.CommandName == "ActivateUser")
        {
            _users.SetActive(userId, true);
            ShowMessage("That account can sign in again.", "alert-success");
            BindGrid();
        }
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
