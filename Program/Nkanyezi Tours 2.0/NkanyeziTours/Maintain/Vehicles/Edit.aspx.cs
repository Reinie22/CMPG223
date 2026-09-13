using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Vehicles;

public partial class Edit : Page
{
    private VehicleRepository _vehicles => Repositories.Vehicles;
    private LookupRepository _lookups => Repositories.Lookups;

    private int? VehicleId =>
        int.TryParse(Request.QueryString["id"], out var id) ? id : (int?)null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
            return;

        BindRegions();

        if (VehicleId == null)
            return;

        var vehicle = _vehicles.GetById(VehicleId.Value);

        if (vehicle == null)
        {
            Response.Redirect("~/Maintain/Vehicles/Default.aspx");
            return;
        }

        litHeading.Text     = "Edit " + Server.HtmlEncode(vehicle.Display);
        txtReg.Text         = vehicle.RegistrationNumber;
        txtMake.Text        = vehicle.Make;
        txtModel.Text       = vehicle.Model;
        txtColour.Text      = vehicle.Colour;
        txtDescription.Text = vehicle.VehicleDescription;
        txtSeats.Text       = vehicle.Capacity?.ToString() ?? "";
        txtDepot.Text       = vehicle.Location;
        ddlRegion.SelectedValue = vehicle.ProvinceId?.ToString() ?? "";
        txtPrice.Text       = vehicle.PricePerDay.ToString("0.00", CultureInfo.InvariantCulture);
        txtInclKm.Text      = vehicle.IncludedKmPerDay.ToString();
        txtExtraKm.Text     = vehicle.RatePerExtraKm.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private void BindRegions()
    {
        ddlRegion.DataSource = _lookups.GetProvinces();
        ddlRegion.DataTextField = "Name";
        ddlRegion.DataValueField = "Id";
        ddlRegion.DataBind();
        ddlRegion.Items.Insert(0, new ListItem("Choose a region", ""));
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var registration = txtReg.Text.Trim();

        if (_vehicles.RegistrationExists(registration, VehicleId))
        {
            ShowMessage("Another vehicle is already captured with that registration number.",
                        "alert-danger");
            return;
        }

        var vehicle = new Vehicle
        {
            RegistrationNumber = registration,
            Make               = txtMake.Text.Trim(),
            Model              = txtModel.Text.Trim(),
            Colour             = txtColour.Text.Trim(),
            VehicleDescription = txtDescription.Text.Trim(),
            Capacity           = int.TryParse(txtSeats.Text.Trim(), out var seats) ? seats : (int?)null,
            Location           = txtDepot.Text.Trim(),
            ProvinceId         = int.TryParse(ddlRegion.SelectedValue, out var region)
                                     ? region : (int?)null,
            PricePerDay        = decimal.Parse(txtPrice.Text.Trim(), CultureInfo.InvariantCulture),
            IncludedKmPerDay   = int.Parse(txtInclKm.Text.Trim()),
            RatePerExtraKm     = decimal.Parse(txtExtraKm.Text.Trim(), CultureInfo.InvariantCulture)
        };

        if (VehicleId.HasValue)
        {
            vehicle.VehicleId = VehicleId.Value;
            _vehicles.Update(vehicle);
        }
        else
        {
            _vehicles.Create(vehicle);
        }

        Response.Redirect("~/Maintain/Vehicles/Default.aspx?saved=1");
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
