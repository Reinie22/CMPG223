using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

public partial class ChooseUnit : Page
{
    private RoomRepository _rooms => Repositories.Rooms;
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private TripLegRepository _legs => Repositories.TripLegs;
    private AuthService _auth => Repositories.Auth;

    private AppUser? _user;
    private TripLeg? _leg;

    private int? TripLegId =>
        int.TryParse(Request.QueryString["tripLegId"], out var id) ? id : (int?)null;

    private sealed class UnitCard
    {
        public int RoomId { get; set; }
        public string UnitName { get; set; } = "";
        public string TypeDisplay { get; set; } = "";
        public string SleepsDisplay { get; set; } = "";
        public string BedsDisplay { get; set; } = "";
        public string BathroomDisplay { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public string TotalDisplay { get; set; } = "";
        public bool IsChosen { get; set; }
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

        var tripUrl = "~/Trips/MyTrip.aspx?id=" + _leg!.ItineraryId;
        lnkBack.NavigateUrl = tripUrl;
        lnkDone.NavigateUrl = tripUrl;

        lnkChangeResort.NavigateUrl =
            "~/Trips/ChooseAccommodation.aspx?tripLegId=" + _leg.TripLegId;
        lnkChangeResort.Text = "Not " + Server.HtmlEncode(_leg.ResortName ?? "this resort")
                             + "? Choose a different resort";

        if (!IsPostBack)
            Bind();
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
            ShowMessage("This trip is finished, so it cannot be changed.", "alert-warning");
            return false;
        }

        if (!_leg.HasResort)
        {
            Response.Redirect("~/Trips/ChooseAccommodation.aspx?tripLegId=" + _leg.TripLegId);
            return false;
        }

        return true;
    }

    private void Bind()
    {
        litSubtitle.Text = Server.HtmlEncode(
            $"{_leg!.ResortName} - {_leg.Title}, {_leg.DateDisplay}, {_leg.Guests} guests");

        BindSelection();

        var chosen = new HashSet<int>(_legs.GetRoomIds(_leg.TripLegId));

        var units = _rooms.GetAvailable(
            _leg.AccommodationId!.Value, _leg.StartDate, _leg.EndDate);

        pnlNone.Visible = units.Count == 0;

        rptUnits.DataSource = units.Select(u => new UnitCard
        {
            RoomId          = u.RoomId,
            UnitName        = u.RoomNumber,
            TypeDisplay     = string.IsNullOrWhiteSpace(u.RoomType) ? "Room" : u.RoomType!,
            SleepsDisplay   = "Capacity " + u.Capacity,
            BedsDisplay     = u.Beds + (u.Beds == 1 ? " bed" : " beds"),
            BathroomDisplay = u.Bathroom ? "Private bathroom" : "Shared bathroom",
            PricePerNight   = u.PricePerNight,
            TotalDisplay    = $"R{u.PricePerNight * _leg.Nights:N2} for {_leg.Nights} "
                              + (_leg.Nights == 1 ? "night" : "nights"),
            IsChosen        = chosen.Contains(u.RoomId)
        }).ToList();

        rptUnits.DataBind();
    }

    private void BindSelection()
    {
        const string layout = " py-2 d-flex flex-wrap justify-content-between align-items-center gap-2";

        if (_leg!.RoomCount == 0)
        {
            pnlSelection.CssClass = "alert alert-light border" + layout;
            litSelection.Text = Server.HtmlEncode(
                $"No rooms chosen yet. This leg has {_leg.Guests} guests.");
            lnkDone.Visible = false;
            return;
        }

        var shortBy = _leg.Guests - _leg.RoomsCapacity;

        var summary = $"{_leg.RoomCount} room" + (_leg.RoomCount == 1 ? "" : "s")
                    + $" chosen - capacity {_leg.RoomsCapacity} for {_leg.Guests} guests"
                    + $" - R{_leg.EstimatedResortCost:N2} for {_leg.Nights} "
                    + (_leg.Nights == 1 ? "night." : "nights.");

        if (shortBy > 0)
            summary += $" Add rooms for {shortBy} more " + (shortBy == 1 ? "guest." : "guests.");

        pnlSelection.CssClass = (shortBy > 0 ? "alert alert-danger" : "alert alert-success") + layout;
        litSelection.Text = Server.HtmlEncode(summary);
        lnkDone.Visible = true;
    }

    protected void rptUnits_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "ToggleRoom" || _leg == null)
            return;

        if (!int.TryParse(Convert.ToString(e.CommandArgument), out var roomId))
            return;

        try
        {
            if (_legs.GetRoomIds(_leg.TripLegId).Contains(roomId))
            {
                _legs.RemoveRoom(_leg.TripLegId, roomId);
            }
            else if (!_legs.AddRoom(_leg.TripLegId, roomId))
            {
                ShowMessage("That room is not part of this resort. Please choose again.",
                            "alert-danger");
            }

            _leg = _legs.GetById(_leg.TripLegId);
            Bind();
        }
        catch (Exception ex)
        {
            ShowMessage("Could not save your rooms. " + Server.HtmlEncode(ex.Message),
                        "alert-danger");
            Bind();
        }
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
