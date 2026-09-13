using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Vehicles;

public partial class Maintenance : Page
{
    private VehicleRepository _vehicles => Repositories.Vehicles;
    private VehicleMaintenanceRepository _maintenance => Repositories.VehicleMaintenance;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        try
        {
            txtStartDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtEndDate.Text   = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

            BindVehicles();
            BindDashboard();
        }
        catch (Exception ex)
        {
            ShowMessage("Could not load the fleet. " + Server.HtmlEncode(ex.Message),
                        "alert-danger");
        }
    }

    protected void btnSchedule_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        if (!int.TryParse(ddlVehicle.SelectedValue, out var vehicleId) ||
            !TryDate(txtStartDate.Text, out var startDate) ||
            !TryDate(txtEndDate.Text, out var endDate))
        {
            ShowMessage("Choose a vehicle and give valid dates.", "alert-danger");
            return;
        }

        if (endDate < startDate)
        {
            ShowMessage("The work cannot end before it starts.", "alert-danger");
            return;
        }

        if (!TryOptionalInt(txtOdometer.Text, out var odometer))
        {
            ShowMessage("The odometer reading has to be a whole number of kilometres.",
                        "alert-danger");
            return;
        }

        if (!TryOptionalMoney(txtEstimatedCost.Text, out var estimatedCost))
        {
            ShowMessage("The estimated cost has to be an amount, for example 1250.00.",
                        "alert-danger");
            return;
        }

        try
        {
            var id = _maintenance.Create(new VehicleMaintenance
            {
                VehicleId       = vehicleId,
                MaintenanceType = txtType.Text,
                StartDate       = startDate,
                EndDate         = endDate,
                Odometer        = odometer,
                EstimatedCost   = estimatedCost,
                Notes           = txtNotes.Text,
                CreatedBy       = AuthService.CurrentUserId
            });

            if (id == 0)
            {
                ShowMessage("That car is already hired out or already in for work over "
                            + "those dates. Pick another period.", "alert-danger");
                return;
            }

            ClearEntryForm();
            BindDashboard();
            ShowMessage("Car booked in. It will not show as available while the work is on.",
                        "alert-success");
        }
        catch (Exception ex)
        {
            ShowMessage("Could not book the car in. " + Server.HtmlEncode(ex.Message),
                        "alert-danger");
        }
    }

    protected void rptMaintenance_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "Complete" ||
            !int.TryParse(Convert.ToString(e.CommandArgument), out var maintenanceId))
            return;

        try
        {
            var completed = _maintenance.Complete(maintenanceId, null);

            BindDashboard();

            ShowMessage(completed
                            ? "Work closed. The car is back on hire."
                            : "That job had already been closed.",
                        completed ? "alert-success" : "alert-warning");
        }
        catch (Exception ex)
        {
            ShowMessage("Could not close the job. " + Server.HtmlEncode(ex.Message),
                        "alert-danger");
        }
    }

    private void BindVehicles()
    {
        ddlVehicle.Items.Clear();
        ddlVehicle.Items.Add(new ListItem("Choose a car", ""));

        foreach (var vehicle in _vehicles.GetAll())
        {
            var branch = string.IsNullOrWhiteSpace(vehicle.Location)
                ? "No branch"
                : vehicle.Location;

            ddlVehicle.Items.Add(new ListItem(
                $"{vehicle.Display} - {vehicle.RegistrationNumber} - {branch}",
                vehicle.VehicleId.ToString()));
        }
    }

    private void BindDashboard()
    {
        litSummaryDate.Text = Server.HtmlEncode(DateTime.Today.ToString("dd/MM/yyyy"));

        rptLocations.DataSource = _maintenance.GetFleetSummary(DateTime.Today);
        rptLocations.DataBind();

        var records = _maintenance.GetAll();
        rptMaintenance.DataSource = records;
        rptMaintenance.DataBind();
        pnlNoMaintenance.Visible = records.Count == 0;
    }

    private void ClearEntryForm()
    {
        ddlVehicle.SelectedIndex = 0;
        txtType.Text             = "";
        txtStartDate.Text        = DateTime.Today.ToString("yyyy-MM-dd");
        txtEndDate.Text          = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
        txtOdometer.Text         = "";
        txtEstimatedCost.Text    = "";
        txtNotes.Text            = "";
    }

    private static bool TryDate(string value, out DateTime date)
        => DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out date);

    private static bool TryOptionalInt(string text, out int? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(text)) return true;

        if (!int.TryParse(text.Trim(), out var parsed) || parsed < 0) return false;

        value = parsed;
        return true;
    }

    private static bool TryOptionalMoney(string text, out decimal? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(text)) return true;

        var t = text.Trim();

        var ok = decimal.TryParse(t, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed)
              || decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);

        if (!ok || parsed < 0) return false;

        value = parsed;
        return true;
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
