using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NkanyeziTours.Itineraries;

public partial class BookRoute : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private RouteRepository _routes => Repositories.Routes;
    private RouteBookingRepository _bookings => Repositories.RouteBookings;

    private int? ItineraryId =>
        int.TryParse(Request.QueryString["itineraryId"], out var id) ? id : (int?)null;

    private string? RouteType
    {
        get
        {
            var type = Request.QueryString["type"];
            if (string.Equals(type, RouteTypes.Flight, StringComparison.OrdinalIgnoreCase))
                return RouteTypes.Flight;
            if (string.Equals(type, RouteTypes.Shuttle, StringComparison.OrdinalIgnoreCase))
                return RouteTypes.Shuttle;
            return null;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        try
        {
            var trip = LoadEditableTrip();
            if (trip == null || RouteType == null)
            {
                if (RouteType == null) ShowError("Choose either bus or aeroplane transport.", true);
                return;
            }

            var isFlight = RouteType == RouteTypes.Flight;
            litHeading.Text = isFlight ? "Book an aeroplane" : "Book a bus or shuttle";
            litRouteLabel.Text = isFlight ? "Flight route" : "Bus or shuttle route";
            btnBook.Text = isFlight ? "Book flight" : "Book bus";
            litTrip.Text = Server.HtmlEncode($"{trip.TripName} · {trip.ClientName}");
            txtTravelDate.Text = (trip.StartDate ?? DateTime.Today).ToString("yyyy-MM-dd");
            lnkCancel.NavigateUrl = ChooserUrl();
            LoadRoutes(trip.StartDate ?? DateTime.Today);
        }
        catch (Exception ex)
        {
            ShowError("Could not load routes. " + Server.HtmlEncode(ex.Message), true);
        }
    }

    protected void ddlRoute_SelectedIndexChanged(object sender, EventArgs e)
    {
        ShowSelectedRoute();
    }

    protected void txtTravelDate_TextChanged(object sender, EventArgs e)
    {
        if (!TryGetTravelDate(out var travelDate))
        {
            ShowError("Choose a valid travel date.");
            return;
        }

        try
        {
            LoadRoutes(travelDate);
        }
        catch (Exception ex)
        {
            ShowError("Could not check seat availability. " + Server.HtmlEncode(ex.Message));
        }
    }

    protected void cvFare_ServerValidate(object source, ServerValidateEventArgs args)
    {
        args.IsValid = TryParseMoney(args.Value, out var amount) && amount > 0;
    }

    protected void btnBook_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        try
        {
            if (LoadEditableTrip() == null || RouteType == null) return;

            if (!int.TryParse(ddlRoute.SelectedValue, out var routeId))
            {
                ShowError("Choose a route.");
                return;
            }

            if (!TryGetTravelDate(out var travelDate) ||
                !int.TryParse(txtSeats.Text, out var seats) || seats < 1 ||
                !TryParseMoney(txtFare.Text, out var fare) || fare <= 0)
            {
                ShowError("Enter a valid date, number of seats, and fare.");
                return;
            }

            var route = _routes.GetAvailability(routeId, travelDate);
            if (route == null || route.RouteType != RouteType || !route.IsActive)
            {
                ShowError("That route is no longer available. Choose another route.");
                return;
            }

            if (route.SeatsAvailable == null || seats > route.SeatsAvailable.Value)
            {
                ShowError($"Only {route.SeatsAvailable ?? 0} seat(s) remain for that journey. Choose fewer seats or another service.");
                ShowSelectedRoute();
                return;
            }

            var reference = GenerateReference(RouteType);

            var bookingId = _bookings.Create(new RouteBooking
            {
                RouteId = routeId,
                ItineraryId = ItineraryId!.Value,
                TravelDate = travelDate,
                Seats = seats,
                FarePerSeat = fare,
                BookingReference = reference,
                Status = BookingStatus.Confirmed,
                CreatedBy = AuthService.CurrentUserId
            });

            if (bookingId == 0)
            {
                ShowError("Those seats were just taken by another booking. The availability has been refreshed.");
                LoadRoutes(travelDate);
                return;
            }

            var booked = RouteType == RouteTypes.Flight ? "flight" : "bus";
            Response.Redirect(ChooserUrl() + "&booked=" + booked
                              + "&reference=" + Server.UrlEncode(reference), false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {
            ShowError("Could not save the transport booking. " + Server.HtmlEncode(ex.Message));
        }
    }

    private Itinerary? LoadEditableTrip()
    {
        if (ItineraryId == null)
        {
            ShowError("Choose an itinerary before booking transport.", true);
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
            ShowError("Transport cannot be added to a completed or cancelled itinerary.", true);
            return null;
        }

        return trip;
    }

    private static bool TryParseMoney(string value, out decimal amount) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out amount);

    private bool TryGetTravelDate(out DateTime travelDate) =>
        DateTime.TryParseExact(txtTravelDate.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out travelDate);

    private void LoadRoutes(DateTime travelDate)
    {
        if (RouteType == null) return;

        var isFlight = RouteType == RouteTypes.Flight;
        var routes = _routes.GetAvailabilityByType(RouteType, travelDate);
        ddlRoute.DataSource = routes;
        ddlRoute.DataTextField = "BookingDisplay";
        ddlRoute.DataValueField = "RouteId";
        ddlRoute.DataBind();
        ddlRoute.Items.Insert(0, new ListItem(isFlight ? "Choose a flight" : "Choose a bus or shuttle", ""));

        pnlAvailability.Visible = false;
        txtFare.Text = "";
        btnBook.Enabled = routes.Count > 0;

        if (routes.Count == 0)
            ShowError(isFlight ? "There are no flight routes for this date." : "There are no bus or shuttle routes for this date.");
    }

    private void ShowSelectedRoute()
    {
        pnlAvailability.Visible = false;

        if (!int.TryParse(ddlRoute.SelectedValue, out var routeId) || !TryGetTravelDate(out var travelDate))
            return;

        var route = _routes.GetAvailability(routeId, travelDate);
        if (route == null || route.RouteType != RouteType) return;

        var available = Math.Max(0, route.SeatsAvailable ?? 0);
        var used = Math.Max(0, route.SeatCapacity - available);
        var usedPercent = route.SeatCapacity == 0 ? 100 : used * 100 / route.SeatCapacity;

        txtFare.Text = route.IndicativeFarePerSeat.ToString("0.00", CultureInfo.InvariantCulture);
        litAvailability.Text = $"<strong>{available}</strong> of {route.SeatCapacity} seats available on {travelDate:dd/MM/yyyy}";
        lblAvailabilityStatus.Text = available > 0 ? "Available" : "Fully booked";
        lblAvailabilityStatus.CssClass = "badge rounded-pill " + (available > 0 ? "bg-success" : "bg-danger");
        pnlAvailabilityFill.Style["width"] = Math.Min(100, usedPercent) + "%";
        pnlAvailability.Visible = true;

        rvSeats.MaximumValue = Math.Max(1, available).ToString(CultureInfo.InvariantCulture);
        rvSeats.ErrorMessage = available > 0
            ? $"Choose between 1 and {available} available seats."
            : "This service is fully booked.";
        btnBook.Enabled = available > 0;
    }

    private static string GenerateReference(string routeType)
    {
        var mode = routeType == RouteTypes.Flight ? "FLT" : "BUS";
        var unique = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        return $"NKT-{mode}-{DateTime.Today:yyMMdd}-{unique}";
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
