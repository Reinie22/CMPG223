using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NkanyeziTours.Maintain.Transport;

public partial class Default : Page
{
    private readonly VehicleRepository _vehicles = Repositories.Vehicles;
    private readonly VehicleMaintenanceRepository _maintenance = Repositories.VehicleMaintenance;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        txtStartDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
        txtEndDate.Text = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
        BindVehicles();
        BindDashboard();
    }

    protected void btnSchedule_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        if (!int.TryParse(ddlVehicle.SelectedValue, out var vehicleId) ||
            !TryDate(txtStartDate.Text, out var startDate) ||
            !TryDate(txtEndDate.Text, out var endDate))
        {
            ShowMessage("Choose a vehicle and valid maintenance dates.", false);
            return;
        }

        if (endDate < startDate)
        {
            ShowMessage("The maintenance end date cannot be before its start date.", false);
            return;
        }

        int? odometer = null;
        if (!string.IsNullOrWhiteSpace(txtOdometer.Text))
        {
            if (!int.TryParse(txtOdometer.Text, out var parsedOdometer) || parsedOdometer < 0)
            {
                ShowMessage("Enter a valid odometer reading.", false);
                return;
            }
            odometer = parsedOdometer;
        }

        decimal? estimatedCost = null;
        if (!string.IsNullOrWhiteSpace(txtEstimatedCost.Text))
        {
            var validCost = decimal.TryParse(txtEstimatedCost.Text, NumberStyles.Number,
                                CultureInfo.CurrentCulture, out var parsedCost)
                         || decimal.TryParse(txtEstimatedCost.Text, NumberStyles.Number,
                                CultureInfo.InvariantCulture, out parsedCost);
            if (!validCost || parsedCost < 0)
            {
                ShowMessage("Enter a valid estimated cost.", false);
                return;
            }
            estimatedCost = parsedCost;
        }

        try
        {
            var id = _maintenance.Create(new VehicleMaintenance
            {
                VehicleId = vehicleId,
                MaintenanceType = txtType.Text,
                StartDate = startDate,
                EndDate = endDate,
                Odometer = odometer,
                EstimatedCost = estimatedCost,
                Notes = txtNotes.Text,
                CreatedBy = AuthService.CurrentUserId
            });

            if (id == 0)
            {
                ShowMessage("That vehicle already has a booking or maintenance during those dates. Choose another period.", false);
                return;
            }

            ClearEntryForm();
            BindDashboard();
            ShowMessage("Maintenance scheduled. The car is now unavailable for bookings during that period.", true);
        }
        catch (Exception ex)
        {
            ShowMessage("Could not schedule maintenance. " + Server.HtmlEncode(ex.Message), false);
        }
    }

    protected void rptMaintenance_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "Complete" ||
            !int.TryParse(Convert.ToString(e.CommandArgument), out var maintenanceId)) return;

        try
        {
            var completed = _maintenance.Complete(maintenanceId, null);
            BindDashboard();
            ShowMessage(completed ? "Maintenance marked as complete. The car can be booked again."
                                  : "That maintenance record was already completed.", completed);
        }
        catch (Exception ex)
        {
            ShowMessage("Could not complete maintenance. " + Server.HtmlEncode(ex.Message), false);
        }
    }

    private void BindVehicles()
    {
        ddlVehicle.Items.Clear();
        ddlVehicle.Items.Add(new ListItem("Choose a rental car", ""));
        foreach (var vehicle in _vehicles.GetAll())
        {
            var location = string.IsNullOrWhiteSpace(vehicle.Location) ? "No branch" : vehicle.Location;
            ddlVehicle.Items.Add(new ListItem(
                $"{vehicle.Display} · {vehicle.RegistrationNumber} · {location}",
                vehicle.VehicleId.ToString()));
        }
    }

    private void BindDashboard()
    {
        litSummaryDate.Text = DateTime.Today.ToString("dd MMMM yyyy");
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
        txtType.Text = "";
        txtStartDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
        txtEndDate.Text = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
        txtOdometer.Text = "";
        txtEstimatedCost.Text = "";
        txtNotes.Text = "";
    }

    private static bool TryDate(string value, out DateTime date)
        => DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out date);

    private void ShowMessage(string message, bool success)
    {
        pnlMessage.CssClass = success ? "alert alert-success" : "alert alert-danger";
        litMessage.Text = message;
        pnlMessage.Visible = true;
    }
}
