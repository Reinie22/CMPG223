using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

public partial class ChooseTransport : Page
{
    private VehicleRepository _vehicles => Repositories.Vehicles;
    private RouteRepository _routes => Repositories.Routes;
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private TripRequestRepository _requests => Repositories.TripRequests;
    private TripLegRepository _legs => Repositories.TripLegs;
    private AuthService _auth => Repositories.Auth;

    private AppUser? _user;
    private TripLeg? _leg;

    private int? TripLegId =>
        int.TryParse(Request.QueryString["tripLegId"], out var id) ? id : (int?)null;

    private const int ModeCars = 0, ModeFlights = 1, ModeBuses = 2;

    private int Mode
    {
        get => ViewState["Mode"] as int? ?? ModeCars;
        set => ViewState["Mode"] = value;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        _user = _auth.GetCurrentUser();

        if (_user == null)
        {
            Response.Redirect("~/Account/Login.aspx");
            return;
        }

        if (!LoadLeg())
            return;

        lnkBack.NavigateUrl = "~/Trips/MyTrip.aspx?id=" + _leg!.ItineraryId;

        if (!IsPostBack)
        {
            litSubtitle.Text = Server.HtmlEncode($"For {_leg.Title} - {_leg.DateDisplay}");

            litTickets.Text = _leg.Guests + (_leg.Guests == 1 ? " ticket, for the 1 person" : " tickets, one for each of the " + _leg.Guests + " people")
                              + " on this leg";

            BindTravelDays();
            BindCars();
            BindRoutes();
        }

        ShowTab();
    }

    private bool LoadLeg()
    {
        if (TripLegId == null)
        {
            Response.Redirect("~/Default.aspx");
            return false;
        }

        _leg = _legs.GetById(TripLegId.Value);

        if (_leg == null)
        {
            Response.Redirect("~/Default.aspx");
            return false;
        }

        if (!_itineraries.BelongsToClient(_leg.ItineraryId, _user!.UserId))
        {
            pnlBody.Visible = false;
            ShowMessage("That trip is not yours.", "alert-danger");
            return false;
        }

        var trip = _itineraries.GetById(_leg.ItineraryId);

        if (trip == null || !trip.ClientCanEdit)
        {
            pnlBody.Visible = false;
            ShowMessage("This trip can no longer be changed here. Contact your agent.",
                        "alert-warning");
            return false;
        }

        return true;
    }

    private void ShowTab()
    {
        pnlCars.Visible  = Mode == ModeCars;
        pnlSeats.Visible = Mode != ModeCars;

        lnkTabCars.CssClass    = Mode == ModeCars    ? "nav-link active" : "nav-link";
        lnkTabFlights.CssClass = Mode == ModeFlights ? "nav-link active" : "nav-link";
        lnkTabBuses.CssClass   = Mode == ModeBuses   ? "nav-link active" : "nav-link";
    }

    protected void lnkTabCars_Click(object sender, EventArgs e)
    {
        Mode = ModeCars;
        ShowTab();
    }

    protected void lnkTabFlights_Click(object sender, EventArgs e)
    {
        Mode = ModeFlights;
        BindRoutes();
        ShowTab();
    }

    protected void lnkTabBuses_Click(object sender, EventArgs e)
    {
        Mode = ModeBuses;
        BindRoutes();
        ShowTab();
    }

    private void BindTravelDays()
    {
        ddlTravelDate.Items.Clear();

        for (var day = _leg!.StartDate; day < _leg.EndDate; day = day.AddDays(1))
            ddlTravelDate.Items.Add(
                new ListItem(day.ToString("ddd dd/MM/yyyy"), day.ToString("yyyy-MM-dd")));
    }

    private void BindCars()
    {
        var fleet = _vehicles.GetAll();

        if (_leg!.ProvinceId == null)
        {
            litCarRegionNote.Text =
                "This leg has no region set, so every car in the fleet is listed.";
        }
        else
        {
            fleet = fleet.Where(v => v.ProvinceId == _leg.ProvinceId).ToList();

            litCarRegionNote.Text = Server.HtmlEncode(
                $"Cars based in {_leg.ProvinceName}. We do not move cars between "
                + "provinces, so only that region's fleet is on offer.");
        }

        pnlNoCars.Visible = fleet.Count == 0;

        litNoCars.Text = _leg.ProvinceId == null
            ? "There are no cars in the fleet yet."
            : Server.HtmlEncode($"We have no cars based in {_leg.ProvinceName}. "
                                + "Try a bus or a flight instead.");

        rptCars.DataSource = fleet;
        rptCars.DataBind();
    }

    private void BindRoutes()
    {
        var flights = Mode != ModeBuses;

        var routes = _routes.GetByType(flights ? RouteTypes.Flight : RouteTypes.Shuttle);

        litNoRoutes.Text    = flights ? "No flights are scheduled yet." : "No buses are scheduled yet.";
        pnlNoRoutes.Visible = routes.Count == 0;

        rptRoutes.DataSource = routes;
        rptRoutes.DataBind();
    }

    protected void rptCars_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "TakeCar" || _leg == null || !pnlBody.Visible)
            return;

        var vehicleId = Convert.ToInt32(e.CommandArgument);

        _requests.Add(new TripRequestItem
        {
            ItineraryId = _leg.ItineraryId,
            TripLegId   = _leg.TripLegId,
            ItemType    = RequestItemTypes.Vehicle,
            VehicleId   = vehicleId,
            StartDate   = _leg.StartDate,
            EndDate     = _leg.EndDate,
            Guests      = _leg.Guests
        });

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_leg.ItineraryId}&transport=1");
    }

    protected void rptRoutes_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "TakeSeats" || _leg == null || !pnlBody.Visible)
            return;

        var travelDate = ParseDate(ddlTravelDate.SelectedValue);

        if (travelDate == null)
        {
            ShowMessage("Please choose which day you are travelling.", "alert-danger");
            return;
        }

        var seats = Math.Max(_leg.Guests, 1);

        var routeId = Convert.ToInt32(e.CommandArgument);

        _requests.Add(new TripRequestItem
        {
            ItineraryId = _leg.ItineraryId,
            TripLegId   = _leg.TripLegId,
            ItemType    = RequestItemTypes.Route,
            RouteId     = routeId,
            StartDate   = travelDate,
            EndDate     = null,
            Guests      = seats
        });

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_leg.ItineraryId}&transport=1");
    }

    private static DateTime? ParseDate(string value)
        => DateTime.TryParseExact((value ?? "").Trim(), "yyyy-MM-dd",
                                  System.Globalization.CultureInfo.InvariantCulture,
                                  System.Globalization.DateTimeStyles.None, out var result)
               ? result
               : (DateTime?)null;

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
