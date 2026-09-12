using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Accommodation;

public partial class AccommodationForm : Page
{
    private readonly AccommodationRepository _accommodations = Repositories.Accommodation;

    // Confirmed against Maintain/Attractions/Default.aspx.cs
    private readonly LookupRepository _lookups = Repositories.Lookups;

    // Same page is used for both Add (no querystring) and Edit (?id=5)
    private int? AccommodationId =>
        int.TryParse(Request.QueryString["id"], out var id) ? id : (int?)null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindProvinces();

            if (AccommodationId.HasValue)
            {
                litHeading.Text = "Edit Accommodation";
                LoadAccommodation(AccommodationId.Value);
            }
        }
    }

    private void ShowError(string text)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 alert-danger";
        pnlMessage.Visible = true;
    }

    private void BindProvinces()
    {
        ddlProvince.DataSource = _lookups.GetProvinces();
        ddlProvince.DataTextField = "Name";
        ddlProvince.DataValueField = "Id";
        ddlProvince.DataBind();
        ddlProvince.Items.Insert(0, new ListItem("Choose a province", ""));
    }

    private void LoadAccommodation(int id)
    {
        var accommodation = _accommodations.GetById(id);
        if (accommodation == null)
        {
            Response.Redirect("~/Maintain/Accommodation/AccommodationList.aspx");
            return;
        }

        txtName.Text = accommodation.Name;
        txtDescription.Text = accommodation.Description;
        ddlType.SelectedValue = accommodation.Type ?? "";
        txtAddress.Text = accommodation.Address;
        txtCity.Text = accommodation.City;
        ddlProvince.SelectedValue = accommodation.ProvinceId?.ToString() ?? "";
        ddlStarRating.SelectedValue = accommodation.StarRating?.ToString() ?? "";
        txtPricePerNight.Text = accommodation.PricePerNight.ToString("0.00");
        txtCapacity.Text = accommodation.Capacity.ToString();
        txtAmenities.Text = accommodation.Amenities;
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var name = txtName.Text.Trim();

        if (_accommodations.NameExists(name, AccommodationId))
        {
            ShowError("An accommodation with that name already exists.");
            return;
        }

        var accommodation = new Nkanyezi.Models.Accommodation
        {
            Name = name,
            Description = txtDescription.Text.Trim(),
            Type = ddlType.SelectedValue,
            Address = txtAddress.Text.Trim(),
            City = txtCity.Text.Trim(),
            ProvinceId = int.TryParse(ddlProvince.SelectedValue, out var provinceId) ? provinceId : (int?)null,
            StarRating = byte.TryParse(ddlStarRating.SelectedValue, out var stars) ? stars : (byte?)null,
            PricePerNight = decimal.Parse(txtPricePerNight.Text.Trim(), CultureInfo.InvariantCulture),
            Capacity = int.Parse(txtCapacity.Text.Trim()),
            Amenities = txtAmenities.Text.Trim()
        };

        if (AccommodationId.HasValue)
        {
            accommodation.AccommodationId = AccommodationId.Value;
            _accommodations.Update(accommodation);
        }
        else
        {
            _accommodations.Create(accommodation);
        }

        Response.Redirect("~/Maintain/Accommodation/AccommodationList.aspx?saved=1");
    }
}
