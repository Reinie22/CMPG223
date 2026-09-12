using System.Web.UI.WebControls;
using System.Web.UI;
using Nkanyezi.Data;

namespace NkanyeziTours.Maintain.Accommodation;

public partial class AccommodationList : Page
{
    private readonly AccommodationRepository _accommodations = Repositories.Accommodation;

    // Confirmed against Maintain/Attractions/Default.aspx.cs
    private readonly LookupRepository _lookups = Repositories.Lookups;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        if (Request.QueryString["saved"] == "1")
        {
            ShowMessage("Accommodation saved successfully.", "alert-success");
        }

        BindProvinceFilter();
        BindGrid();
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }

    private void BindProvinceFilter()
    {
        ddlSearchProvince.DataSource = _lookups.GetProvinces();
        ddlSearchProvince.DataTextField = "Name";
        ddlSearchProvince.DataValueField = "Id";
        ddlSearchProvince.DataBind();
        ddlSearchProvince.Items.Insert(0, new ListItem("All locations", ""));
    }

    /// <summary>
    /// Search by name, type, location (province), star rating or price - the
    /// fields on the page. Falls back to the full list when nothing is filled in.
    /// </summary>
    private void BindGrid()
    {
        var name = txtSearchName.Text.Trim();
        var type = ddlSearchType.SelectedValue;
        int? provinceId = int.TryParse(ddlSearchProvince.SelectedValue, out var pid) ? pid : (int?)null;
        byte? minStars = byte.TryParse(ddlSearchMinStars.SelectedValue, out var stars) ? stars : (byte?)null;
        decimal? maxPrice = decimal.TryParse(txtSearchMaxPrice.Text.Trim(), out var price) ? price : (decimal?)null;

        var hasFilter = name.Length > 0 || !string.IsNullOrEmpty(type)
                        || provinceId.HasValue || minStars.HasValue || maxPrice.HasValue;

        gvAccommodations.DataSource = hasFilter
            ? _accommodations.Search(name, provinceId, minStars, maxPrice, type, includeInactive: true)
            : _accommodations.GetAll(includeInactive: true);

        gvAccommodations.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindGrid();
    }

    protected void btnClearSearch_Click(object sender, EventArgs e)
    {
        txtSearchName.Text = "";
        ddlSearchType.SelectedIndex = 0;
        ddlSearchProvince.SelectedIndex = 0;
        ddlSearchMinStars.SelectedIndex = 0;
        txtSearchMaxPrice.Text = "";
        BindGrid();
    }

    protected void gvAccommodations_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        var accommodationId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "EditAccommodation")
        {
            Response.Redirect($"~/Maintain/Accommodation/AccommodationForm.aspx?id={accommodationId}");
        }
        else if (e.CommandName == "DeleteAccommodation")
        {
            var deactivatedInstead = _accommodations.Delete(accommodationId);
            ShowMessage(deactivatedInstead
                ? "This accommodation has existing room bookings, so it was deactivated rather than deleted."
                : "Accommodation deleted.", "alert-success");
            BindGrid();
        }
        else if (e.CommandName == "ReactivateAccommodation")
        {
            _accommodations.Reactivate(accommodationId);
            ShowMessage("Accommodation reactivated.", "alert-success");
            BindGrid();
        }
    }
}
