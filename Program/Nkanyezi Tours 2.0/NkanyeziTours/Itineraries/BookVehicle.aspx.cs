using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NkanyeziTours.Itineraries;

public partial class BookVehicle : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private VehicleRepository _vehicles => Repositories.Vehicles;
    private VehicleBookingRepository _bookings => Repositories.VehicleBookings;

    private int? ItineraryId =>
        int.TryParse(Request.QueryString["itineraryId"], out var id) ? id : (int?)null;

    private sealed class CarCard
    {
        public int VehicleId { get; set; }
        public string Display { get; set; } = "";
        public string RegistrationNumber { get; set; } = "";
        public string Location { get; set; } = "";
        public string Capacity { get; set; } = "Not specified";
        public decimal PricePerDay { get; set; }
        public bool IsSelected { get; set; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        try
        {
            var trip = LoadEditableTrip();
            if (trip == null) return;

            litTrip.Text = Server.HtmlEncode($"{trip.TripName} · {trip.ClientName}");
            txtStartDate.Text = (trip.StartDate ?? DateTime.Today).ToString("yyyy-MM-dd");
            txtEndDate.Text = (trip.EndDate ?? trip.StartDate ?? DateTime.Today).ToString("yyyy-MM-dd");
            lnkCancel.NavigateUrl = ChooserUrl();
        }
        catch (Exception ex)
        {
            ShowError("Could not load the car booking page. " + Server.HtmlEncode(ex.Message), true);
        }
    }

    protected void btnFind_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        try
        {
            if (LoadEditableTrip() == null || !TryGetDates(out var start, out var end)) return;

            var available = BindAvailableCars(start, end, null);
            hfVehicleId.Value = "";
            btnBook.Visible = false;
            pnlRate.Visible = false;

            if (available.Count == 0)
                ShowError("No cars are available for those dates. Try a different date range.");
        }
        catch (Exception ex)
        {
            ShowError("Could not search for cars. " + Server.HtmlEncode(ex.Message));
        }
    }

    protected void rptVehicles_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "SelectCar" ||
            !int.TryParse(Convert.ToString(e.CommandArgument), out var vehicleId) ||
            !TryGetDates(out var start, out var end)) return;

        if (!_vehicles.IsAvailable(vehicleId, start, end))
        {
            ShowError("That car is no longer available for those dates. "
                    + "Search again to see what is left.");
            BindAvailableCars(start, end, null);
            hfVehicleId.Value = "";
            btnBook.Visible = false;
            return;
        }

        hfVehicleId.Value = vehicleId.ToString();
        BindAvailableCars(start, end, vehicleId);
        btnBook.Visible = true;
        ShowSelectedRate();
    }

    protected void btnBook_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        try
        {
            if (LoadEditableTrip() == null || !TryGetDates(out var start, out var end)) return;
            if (!int.TryParse(hfVehicleId.Value, out var vehicleId))
            {
                ShowError("Choose an available car.");
                return;
            }

            var bookingId = _bookings.Create(new VehicleBooking
            {
                VehicleId = vehicleId,
                ItineraryId = ItineraryId!.Value,
                StartDate = start,
                EndDate = end,
                Status = BookingStatus.Confirmed,
                CreatedBy = AuthService.CurrentUserId
            });

            if (bookingId == 0)
            {
                ShowError("That car was booked by somebody else. Search again to see current availability.");
                btnBook.Visible = false;
                return;
            }

            Response.Redirect(ChooserUrl() + "&booked=car", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {
            ShowError("Could not save the car booking. " + Server.HtmlEncode(ex.Message));
        }
    }

    private Itinerary? LoadEditableTrip()
    {
        if (ItineraryId == null)
        {
            ShowError("Choose an itinerary before booking a car.", true);
            return null;
        }

        var trip = _itineraries.GetById(ItineraryId.Value);
        if (trip == null)
        {
            ShowError("That itinerary no longer exists.", true);
            return null;
        }

        if (!trip.IsEditable)
        {
            ShowError("A car cannot be added to a completed or cancelled itinerary.", true);
            return null;
        }

        return trip;
    }

    private bool TryGetDates(out DateTime start, out DateTime end)
    {
        var validStart = DateTime.TryParseExact(txtStartDate.Text, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out start);
        var validEnd = DateTime.TryParseExact(txtEndDate.Text, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out end);

        if (!validStart || !validEnd)
        {
            ShowError("Enter valid collection and return dates.");
            return false;
        }

        if (end < start)
        {
            ShowError("The return date cannot be earlier than the collection date.");
            return false;
        }

        return true;
    }

    private void ShowSelectedRate()
    {
        if (!int.TryParse(hfVehicleId.Value, out var vehicleId) ||
            !TryGetDates(out var start, out var end))
        {
            pnlRate.Visible = false;
            return;
        }

        var vehicle = _vehicles.GetById(vehicleId);
        if (vehicle == null)
        {
            pnlRate.Visible = false;
            return;
        }

        var days = (end - start).Days + 1;
        var baseCost = days * vehicle.PricePerDay;
        litRate.Text = Server.HtmlEncode(vehicle.RateDisplay)
                     + $"<br /><strong>Estimated base cost: R{baseCost:N2}</strong> for {days} day"
                     + (days == 1 ? "" : "s")
                     + ". Extra kilometre charges are calculated when the car is returned.";
        pnlRate.Visible = true;
    }

    private List<Vehicle> BindAvailableCars(DateTime start, DateTime end, int? selectedVehicleId)
    {
        var available = _vehicles.GetAvailable(start, end);

        rptVehicles.DataSource = available.Select(vehicle => new CarCard
        {
            VehicleId          = vehicle.VehicleId,
            Display            = vehicle.Display,
            RegistrationNumber = vehicle.RegistrationNumber,
            Location           = string.IsNullOrWhiteSpace(vehicle.Location)
                                     ? "Branch not assigned"
                                     : vehicle.Location!,
            Capacity           = vehicle.Capacity.HasValue
                                     ? $"{vehicle.Capacity.Value} people"
                                     : "Not specified",
            PricePerDay        = vehicle.PricePerDay,
            IsSelected         = selectedVehicleId == vehicle.VehicleId
        }).ToList();

        rptVehicles.DataBind();
        pnlVehicle.Visible = true;

        return available;
    }

    private string ChooserUrl() => $"BookTransport.aspx?itineraryId={ItineraryId}";

    private void ShowError(string message, bool hideForm = false)
    {
        pnlMessage.CssClass = "alert alert-danger py-2";
        litMessage.Text = message;
        pnlMessage.Visible = true;
        if (hideForm) pnlForm.Visible = false;
    }
}
