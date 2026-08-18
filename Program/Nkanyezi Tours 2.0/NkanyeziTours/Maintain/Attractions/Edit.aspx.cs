using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NkanyeziTours.Maintain.Attractions;

/// <summary>
/// Add or edit one attraction.
///
/// THIS IS THE EXAMPLE TO COPY when you build your own form page.
///
/// One page does both jobs. With no id in the address bar it adds a new record;
/// with ?id=7 it loads number 7 and saves over it. Building two nearly
/// identical pages would mean two places to fix every bug.
/// </summary>
public partial class Edit : Page
{
    private readonly AttractionRepository _attractions = Repositories.Attractions;
    private readonly LookupRepository _lookups = Repositories.Lookups;

    /// <summary>
    /// The record being edited, or null when adding a new one.
    /// Read from the address bar rather than kept in ViewState, so a refresh
    /// cannot leave the page editing something different from what it shows.
    /// </summary>
    private int? AttractionId
    {
        get
        {
            var raw = Request.QueryString["id"];
            return int.TryParse(raw, out var id) ? id : (int?)null;
        }
    }

    private bool IsNew => AttractionId == null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        try
        {
            LoadDropDowns();

            litHeading.Text = IsNew ? "Add attraction" : "Edit attraction";

            if (!IsNew)
                LoadExisting(AttractionId!.Value);
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
        ddlCategory.Items.Insert(0, new ListItem("(none)", ""));

        ddlProvince.DataSource = _lookups.GetProvinces();
        ddlProvince.DataTextField = "Name";
        ddlProvince.DataValueField = "Id";
        ddlProvince.DataBind();
        ddlProvince.Items.Insert(0, new ListItem("(none)", ""));
    }

    /// <summary>Fills the form from the database.</summary>
    private void LoadExisting(int id)
    {
        var a = _attractions.GetById(id);

        if (a == null)
        {
            ShowMessage("alert-danger", "That attraction no longer exists. It may have been deleted.");
            btnSave.Enabled = false;
            return;
        }

        txtName.Text        = a.Name;
        txtDescription.Text = a.Description ?? "";
        txtAddress.Text     = a.Address ?? "";
        txtCity.Text        = a.City ?? "";
        txtRate.Text        = a.Rate.ToString("0.00", CultureInfo.InvariantCulture);
        txtQuantity.Text    = a.Quantity.ToString();
        txtUnit.Text        = a.UnitOfMeasure ?? "";
        txtDuration.Text    = a.DurationMinutes?.ToString() ?? "";
        txtSeason.Text      = a.BestOperatingSeason ?? "";
        txtOpens.Text       = FormatTime(a.OperatingTimeStart);
        txtCloses.Text      = FormatTime(a.OperatingTimeEnd);

        SelectValue(ddlCategory, a.CategoryId);
        SelectValue(ddlProvince, a.ProvinceId);
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // The validators on the markup have already run. Page.IsValid is what
        // tells you whether they all passed - without this check the page would
        // happily save invalid data when JavaScript is disabled.
        if (!Page.IsValid) return;

        try
        {
            // ---- rules that the validators cannot express ----

            if (_attractions.NameExists(txtName.Text.Trim(), AttractionId))
            {
                ShowMessage("alert-danger", "Another attraction already uses that name.");
                return;
            }

            var opens  = ParseTime(txtOpens.Text);
            var closes = ParseTime(txtCloses.Text);

            // The database has CHECK (OperatingTimeEnd > OperatingTimeStart), so
            // saving this would throw. Catching it here gives a readable message.
            if (opens != null && closes != null && closes <= opens)
            {
                ShowMessage("alert-danger", "The closing time must be later than the opening time.");
                return;
            }

            var attraction = new Attraction
            {
                AttractionId        = AttractionId ?? 0,
                Name                = txtName.Text.Trim(),
                CategoryId          = ParseInt(ddlCategory.SelectedValue),
                ProvinceId          = ParseInt(ddlProvince.SelectedValue),
                Description         = NullIfBlank(txtDescription.Text),
                Address             = NullIfBlank(txtAddress.Text),
                City                = NullIfBlank(txtCity.Text),
                Rate                = decimal.Parse(txtRate.Text, CultureInfo.InvariantCulture),
                Quantity            = int.Parse(txtQuantity.Text),
                UnitOfMeasure       = NullIfBlank(txtUnit.Text),
                DurationMinutes     = ParseInt(txtDuration.Text),
                OperatingTimeStart  = opens,
                OperatingTimeEnd    = closes,
                BestOperatingSeason = NullIfBlank(txtSeason.Text)
            };

            if (IsNew)
                _attractions.Create(attraction);
            else
                _attractions.Update(attraction);

            Response.Redirect("~/Maintain/Attractions/Default.aspx");
        }
        catch (System.Threading.ThreadAbortException)
        {
            // Response.Redirect always throws this. Let it through untouched,
            // otherwise the catch below would swallow a successful save and
            // report it as a database error.
            throw;
        }
        catch (Exception ex)
        {
            ShowDatabaseError(ex);
        }
    }

    // -------------------------------------------------------------------------
    // Small helpers
    // -------------------------------------------------------------------------

    private static string? NullIfBlank(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseInt(string value)
        => int.TryParse(value, out var result) ? result : (int?)null;

    private static TimeSpan? ParseTime(string value)
        => TimeSpan.TryParse(value, out var result) ? result : (TimeSpan?)null;

    private static string FormatTime(TimeSpan? value)
        => value == null ? "" : value.Value.ToString(@"hh\:mm");

    private static void SelectValue(DropDownList list, int? id)
    {
        var item = list.Items.FindByValue(id?.ToString() ?? "");
        if (item != null)
            list.ClearSelection();
        if (item != null)
            item.Selected = true;
    }

    private void ShowMessage(string cssClass, string html)
    {
        pnlMessage.CssClass = "alert py-2 " + cssClass;
        litMessage.Text = html;
        pnlMessage.Visible = true;
    }

    private void ShowDatabaseError(Exception ex)
    {
        ShowMessage("alert-danger",
            "Could not save. Check that SQL Server is running and that the database has been created."
            + "<br /><small class=\"text-muted\">" + Server.HtmlEncode(ex.Message) + "</small>");
    }
}
