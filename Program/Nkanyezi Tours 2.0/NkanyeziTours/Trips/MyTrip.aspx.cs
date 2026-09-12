using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

/// <summary>
/// The client's own view of one of their trips.
///
/// Two lists, and the difference between them is the whole point:
///
///   "What you have chosen"     - TripRequestItem rows. Wishes. No capacity
///                                held, guide prices only, removable.
///   "Confirmed by your agent"  - real bookings. Capacity held, real prices.
///
/// WHY THIS PAGE IS NOT Itineraries/EditTrip.aspx
/// ==============================================
/// That page is staff-only by Web.config and lets the trip be renamed, the
/// budget changed and the status moved. None of that is a client's to do. This
/// page is read-only about the trip itself and only lets them add or remove
/// their own choices.
/// </summary>
public partial class MyTrip : Page
{
    private readonly ItineraryRepository _itineraries = Repositories.Itineraries;
    private readonly TripRequestRepository _requests = Repositories.TripRequests;
    private readonly AuthService _auth = Repositories.Auth;

    private AppUser? _user;
    private Itinerary? _trip;

    private int? TripId =>
        int.TryParse(Request.QueryString["id"], out var id) ? id : (int?)null;

    protected void Page_Load(object sender, EventArgs e)
    {
        _user = _auth.GetCurrentUser();

        if (_user == null)
        {
            Response.Redirect("~/Account/Login.aspx");
            return;
        }

        if (!LoadTrip())
            return;

        if (!IsPostBack)
        {
            Show();
            ShowWhatJustHappened();
        }
    }

    /// <summary>
    /// Loads the trip and refuses anybody else's.
    ///
    /// BelongsToClient is the check that matters. Without it a client could type
    /// another client's trip id into the address bar and read their itinerary.
    /// </summary>
    private bool LoadTrip()
    {
        if (TripId == null)
        {
            Response.Redirect("~/Default.aspx");
            return false;
        }

        if (!_itineraries.BelongsToClient(TripId.Value, _user!.UserId))
        {
            pnlTrip.Visible = false;
            ShowMessage("That trip is not yours.", "alert-danger");
            return false;
        }

        _trip = _itineraries.GetWithTotals(TripId.Value);

        if (_trip == null)
        {
            Response.Redirect("~/Default.aspx");
            return false;
        }

        return true;
    }

    private void Show()
    {
        litHeading.Text  = Server.HtmlEncode(_trip!.TripName);
        litSubtitle.Text = _trip.AgentName == null
            ? "Waiting for one of our agents to pick up your trip"
            : "Your agent is " + Server.HtmlEncode(_trip.AgentName);

        lblStatus.Text = _trip.Status;
        lblStatus.CssClass = "badge " + _trip.Status switch
        {
            ItineraryStatus.Confirmed => "text-bg-success",
            ItineraryStatus.Completed => "text-bg-primary",
            ItineraryStatus.Cancelled => "text-bg-danger",
            _                         => "text-bg-secondary"
        };

        var requests = _requests.GetByItinerary(_trip.ItineraryId);

        litBudget.Text   = Money(_trip.TotalBudget);
        litBooked.Text   = Money(_trip.BookingsTotal);
        litDates.Text    = _trip.DateRangeDisplay;

        // Declined choices are not going to cost anything, so they are left out
        // of the running total.
        litEstimate.Text = Money(requests
            .Where(r => !r.IsDeclined)
            .Sum(r => r.EstimatedCost));

        gvRequests.DataSource = requests;
        gvRequests.DataBind();

        gvBookings.DataSource = _itineraries.GetBookingLines(_trip.ItineraryId);
        gvBookings.DataBind();

        // A cancelled or completed trip is finished with; nothing more to add.
        pnlAddButtons.Visible = _trip.IsEditable;

        lnkAddAccommodation.NavigateUrl =
            "~/Trips/ChooseAccommodation.aspx?tripId=" + _trip.ItineraryId;
        lnkAddAttraction.NavigateUrl =
            "~/Trips/ChooseAttraction.aspx?tripId=" + _trip.ItineraryId;
    }

    protected void gvRequests_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "RemoveRequest" || _trip == null)
            return;

        var id = Convert.ToInt32(e.CommandArgument);

        // The itinerary id is passed too, so a client cannot remove a row off
        // somebody else's trip by guessing its id.
        if (_requests.RemoveIfPending(id, _trip.ItineraryId))
            ShowMessage("Removed from your trip.", "alert-success");
        else
            ShowMessage("That one has already been dealt with by your agent, so it cannot be removed here.",
                        "alert-warning");

        Show();
    }

    private void ShowWhatJustHappened()
    {
        var q = Request.QueryString;

        if (q["created"] == "1")
            ShowMessage("Your trip is created. Now choose the places you would like to stay "
                      + "and things you would like to do - your agent confirms them.",
                        "alert-success");
        else if (q["added"] == "1")
            ShowMessage("Added to your trip. Your agent will confirm it.", "alert-success");
    }

    private static string Money(decimal amount) => "R" + amount.ToString("N2");

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
