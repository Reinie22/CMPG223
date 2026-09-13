using System.Web.UI;
using System.Web.UI.WebControls;

namespace NkanyeziTours.Maintain.Attractions;

public partial class Default : Page
{
    private AttractionRepository _attractions => Repositories.Attractions;
    private LookupRepository _lookups => Repositories.Lookups;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        try
        {
            LoadDropDowns();
            Bind();
        }
        catch (Exception ex)
        {
            ShowDatabaseError(ex);
        }
    }

    private void LoadDropDowns()
    {
        ddlCategory.DataSource = _lookups.GetAttractionCategories();
        ddlCategory.DataTextField = "Name";
        ddlCategory.DataValueField = "Id";
        ddlCategory.DataBind();
        ddlCategory.Items.Insert(0, new ListItem("All categories", ""));

        ddlProvince.DataSource = _lookups.GetProvinces();
        ddlProvince.DataTextField = "Name";
        ddlProvince.DataValueField = "Id";
        ddlProvince.DataBind();
        ddlProvince.Items.Insert(0, new ListItem("All provinces", ""));
    }

    private void Bind()
    {
        var name       = string.IsNullOrWhiteSpace(txtName.Text) ? null : txtName.Text.Trim();
        var categoryId = ParseInt(ddlCategory.SelectedValue);
        var provinceId = ParseInt(ddlProvince.SelectedValue);
        var maxRate    = ParseDecimal(txtMaxRate.Text);

        gvAttractions.DataSource = _attractions.Search(
            name:            name,
            categoryId:      categoryId,
            provinceId:      provinceId,
            maxRate:         maxRate,
            includeInactive: chkIncludeInactive.Checked);

        gvAttractions.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        try
        {
            gvAttractions.PageIndex = 0;
            Bind();
        }
        catch (Exception ex)
        {
            ShowDatabaseError(ex);
        }
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtName.Text = "";
        ddlCategory.SelectedIndex = 0;
        ddlProvince.SelectedIndex = 0;
        txtMaxRate.Text = "";
        chkIncludeInactive.Checked = false;
        btnSearch_Click(sender, e);
    }

    protected void gvAttractions_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvAttractions.PageIndex = e.NewPageIndex;
        Bind();
    }

    protected void gvAttractions_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "DeleteAttraction") return;

        try
        {
            var id = Convert.ToInt32(e.CommandArgument);
            var wasDeactivated = _attractions.Delete(id);

            if (wasDeactivated)
                ShowMessage("alert-warning",
                    "This attraction has bookings against it, so it was deactivated rather than "
                    + "deleted. Its history stays intact and it no longer appears on booking screens.");
            else
                ShowMessage("alert-success", "The attraction was deleted.");

            Bind();
        }
        catch (Exception ex)
        {
            ShowDatabaseError(ex);
        }
    }

    private static int? ParseInt(string value)
        => int.TryParse(value, out var result) ? result : (int?)null;

    private static decimal? ParseDecimal(string value)
        => decimal.TryParse(value, out var result) ? result : (decimal?)null;

    private void ShowMessage(string cssClass, string html)
    {
        pnlMessage.CssClass = "alert py-2 " + cssClass;
        litMessage.Text = html;
        pnlMessage.Visible = true;
    }

    private void ShowDatabaseError(Exception ex)
    {
        ShowMessage("alert-danger",
            "Could not load attractions. Check that SQL Server is running and that the database "
            + "has been created by running 01-schema.sql and 03-seed-data.sql."
            + "<br /><small class=\"text-muted\">" + Server.HtmlEncode(ex.Message) + "</small>");
    }
}
