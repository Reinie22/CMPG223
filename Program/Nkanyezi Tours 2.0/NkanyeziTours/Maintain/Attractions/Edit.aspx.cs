using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NkanyeziTours.Maintain.Attractions;

public partial class Edit : Page
{
    private AttractionRepository _attractions => Repositories.Attractions;
    private LookupRepository _lookups => Repositories.Lookups;

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
        ddlCategory.Items.Insert(0, new ListItem("Choose a category", ""));

        ddlProvince.DataSource = _lookups.GetProvinces();
        ddlProvince.DataTextField = "Name";
        ddlProvince.DataValueField = "Id";
        ddlProvince.DataBind();
        ddlProvince.Items.Insert(0, new ListItem("Choose a province", ""));
    }

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

        pnlCurrentPhoto.Visible  = !string.IsNullOrWhiteSpace(a.ImageUrl);
        imgCurrentPhoto.ImageUrl = a.ImageUrl ?? "";
        txtOpens.Text       = FormatTime(a.OperatingTimeStart);
        txtCloses.Text      = FormatTime(a.OperatingTimeEnd);

        SelectValue(ddlCategory, a.CategoryId);
        SelectValue(ddlProvince, a.ProvinceId);
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        try
        {
            if (_attractions.NameExists(txtName.Text.Trim(), AttractionId))
            {
                ShowMessage("alert-danger", "Another attraction already uses that name.");
                return;
            }

            var opens  = ParseTime(txtOpens.Text);
            var closes = ParseTime(txtCloses.Text);

            if (opens != null && closes != null && closes <= opens)
            {
                ShowMessage("alert-danger", "The closing time must be later than the opening time.");
                return;
            }

            if (!ImageStore.TrySave(fuPhoto, "activities", out var newPhoto, out var photoError))
            {
                ShowMessage("alert-danger", Server.HtmlEncode(photoError!));
                return;
            }

            var oldPhoto = IsNew ? null : _attractions.GetById(AttractionId!.Value)?.ImageUrl;

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

            int savedId;

            if (IsNew)
            {
                savedId = _attractions.Create(attraction);
            }
            else
            {
                _attractions.Update(attraction);
                savedId = AttractionId!.Value;
            }

            if (newPhoto != null)
            {
                _attractions.SetImageUrl(savedId, newPhoto);
                ImageStore.Delete(oldPhoto);
            }
            else if (chkRemovePhoto.Checked && oldPhoto != null)
            {
                _attractions.SetImageUrl(savedId, null);
                ImageStore.Delete(oldPhoto);
            }

            Response.Redirect("~/Maintain/Attractions/Default.aspx");
        }
        catch (System.Threading.ThreadAbortException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ShowDatabaseError(ex);
        }
    }

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
