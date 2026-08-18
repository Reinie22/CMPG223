using System.Web.UI;
using System.Web.UI.WebControls;

namespace NkanyeziTours.Maintain.Attractions;

/// <summary>
/// The attraction list screen.
///
/// THIS IS THE EXAMPLE TO COPY when you build your own list page.
///
/// Notice what is not here: no SQL, no connection string, no SqlConnection. The
/// page asks the repository for data and hands it to the grid. That is the
/// whole job of a page.
///
/// The pattern is always the same:
///   1. Page_Load fills the dropdowns and binds the grid, but only when
///      IsPostBack is false.
///   2. A button handler re-binds the grid.
///   3. Bind() is the single method that talks to the repository.
/// </summary>
public partial class Default : Page
{
    // The repositories arrive from the static factory that replaced the
    // dependency injection container. You never write "new Db(...)".
    private readonly AttractionRepository _attractions = Repositories.Attractions;
    private readonly LookupRepository _lookups = Repositories.Lookups;

    protected void Page_Load(object sender, EventArgs e)
    {
        // IsPostBack is false the first time the page opens, and true every
        // time a button on it is clicked. Filling the dropdowns again on a
        // postback would wipe out whatever the user had selected.
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

    /// <summary>
    /// Fills the two filter dropdowns from the lookup tables.
    ///
    /// The blank first item matters: it is what "no filter" looks like. Its
    /// value is an empty string, which Bind() turns back into null.
    /// </summary>
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

    /// <summary>
    /// The one method that fetches data. Everything else calls this.
    ///
    /// Every filter is optional. An empty box becomes null, and the repository's
    /// SQL drops that condition - see the "@Parameter IS NULL OR ..." pattern in
    /// AttractionRepository.Search.
    /// </summary>
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
            gvAttractions.PageIndex = 0;   // a new search starts at page one
            Bind();
        }
        catch (Exception ex)
        {
            ShowDatabaseError(ex);
        }
    }

    protected void gvAttractions_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvAttractions.PageIndex = e.NewPageIndex;
        Bind();
    }

    /// <summary>
    /// Handles the Delete link in the grid.
    ///
    /// The repository deactivates instead of deleting when the attraction has
    /// been booked before, and tells us which of the two it did, so the message
    /// can be honest rather than claiming it was deleted while it is still
    /// sitting in the list.
    /// </summary>
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

    // -------------------------------------------------------------------------
    // Small helpers
    // -------------------------------------------------------------------------

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
        // A missing database is the most likely problem on a new machine, so
        // say so on the page rather than showing a yellow crash screen.
        ShowMessage("alert-danger",
            "Could not load attractions. Check that SQL Server is running and that the database "
            + "has been created by running 01-schema.sql and 03-seed-data.sql."
            + "<br /><small class=\"text-muted\">" + Server.HtmlEncode(ex.Message) + "</small>");
    }
}
