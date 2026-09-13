using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;

namespace NkanyeziTours.Maintain.Vehicles;

public partial class _Default : Page
{
    private VehicleRepository _vehicles => Repositories.Vehicles;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        if (Request.QueryString["saved"] == "1")
            ShowMessage("Vehicle saved.", "alert-success");

        BindGrid();
    }

    private void BindGrid()
    {
        var text     = txtSearchText.Text.Trim();
        var location = txtLocation.Text.Trim();
        int? minSeats = int.TryParse(txtMinSeats.Text.Trim(), out var seats) ? seats : (int?)null;

        var hasFilter = text.Length > 0 || location.Length > 0 || minSeats.HasValue;

        gvVehicles.DataSource = hasFilter
            ? _vehicles.Search(text, location, minSeats, maxPricePerDay: null, includeInactive: true)
            : _vehicles.GetAll(includeInactive: true);

        gvVehicles.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e) => BindGrid();

    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtSearchText.Text = "";
        txtLocation.Text   = "";
        txtMinSeats.Text   = "";
        BindGrid();
    }

    protected void gvVehicles_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        var vehicleId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "RetireVehicle")
        {
            var keptAndRetired = _vehicles.Delete(vehicleId);

            ShowMessage(keptAndRetired
                ? "That vehicle has hire bookings against it, so it was retired rather than deleted."
                : "Vehicle deleted.",
                keptAndRetired ? "alert-warning" : "alert-success");

            BindGrid();
        }
        else if (e.CommandName == "ReactivateVehicle")
        {
            _vehicles.Reactivate(vehicleId);
            ShowMessage("Vehicle put back into service.", "alert-success");
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
