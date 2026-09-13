using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Accommodation;

public partial class AccommodationForm : Page
{
    private AccommodationRepository _accommodations => Repositories.Accommodation;

    private LookupRepository _lookups => Repositories.Lookups;

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

        litUnitSummary.Text = accommodation.RoomCount == 0
            ? " This resort has no units captured yet, so clients cannot book it."
            : Server.HtmlEncode(
                  $" Currently {accommodation.RoomCount} unit(s), from "
                  + $"R{accommodation.FromPricePerNight:N2} a night, sleeping "
                  + $"{accommodation.TotalCapacity} in total.");
        txtAmenities.Text = accommodation.Amenities;

        pnlCurrentPhoto.Visible = !string.IsNullOrWhiteSpace(accommodation.ImageUrl);
        imgCurrentPhoto.ImageUrl = accommodation.ImageUrl ?? "";
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

        if (!ImageStore.TrySave(fuPhoto, "accommodation", out var newPhoto, out var photoError))
        {
            ShowError(photoError!);
            return;
        }

        var oldPhoto = AccommodationId.HasValue
            ? _accommodations.GetById(AccommodationId.Value)?.ImageUrl
            : null;

        var accommodation = new Nkanyezi.Models.Accommodation
        {
            Name = name,
            Description = txtDescription.Text.Trim(),
            Type = ddlType.SelectedValue,
            Address = txtAddress.Text.Trim(),
            City = txtCity.Text.Trim(),
            ProvinceId = int.TryParse(ddlProvince.SelectedValue, out var provinceId) ? provinceId : (int?)null,
            StarRating = byte.TryParse(ddlStarRating.SelectedValue, out var stars) ? stars : (byte?)null,

            PricePerNight = 0m,
            Capacity = 0,
            Amenities = txtAmenities.Text.Trim()
        };

        int savedId;

        if (AccommodationId.HasValue)
        {
            accommodation.AccommodationId = AccommodationId.Value;
            _accommodations.Update(accommodation);
            savedId = AccommodationId.Value;
        }
        else
        {
            savedId = _accommodations.Create(accommodation);
        }

        if (newPhoto != null)
        {
            _accommodations.SetImageUrl(savedId, newPhoto);
            ImageStore.Delete(oldPhoto);
        }
        else if (chkRemovePhoto.Checked && oldPhoto != null)
        {
            _accommodations.SetImageUrl(savedId, null);
            ImageStore.Delete(oldPhoto);
        }

        Response.Redirect("~/Maintain/Accommodation/AccommodationList.aspx?saved=1");
    }
}
