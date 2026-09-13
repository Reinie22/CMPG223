using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

public partial class MyTrip : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private TripRequestRepository _requests => Repositories.TripRequests;
    private TripLegRepository _legs => Repositories.TripLegs;
    private LookupRepository _lookups => Repositories.Lookups;
    private AuthService _auth => Repositories.Auth;

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
            BindRegions();
            Show();
            ShowWhatJustHappened();
        }
    }

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

    private void BindRegions()
    {
        ddlRegion.DataSource = _lookups.GetProvinces();
        ddlRegion.DataTextField = "Name";
        ddlRegion.DataValueField = "Id";
        ddlRegion.DataBind();
        ddlRegion.Items.Insert(0, new ListItem("Choose a region", ""));
    }

    private void Show()
    {
        litHeading.Text  = Server.HtmlEncode(_trip!.TripName);
        litSubtitle.Text = _trip.AgentName == null
            ? "Agent to be assigned"
            : "Your agent is " + Server.HtmlEncode(_trip.AgentName);

        lblStatus.Text = _trip.Status;
        lblStatus.CssClass = "badge " + _trip.Status switch
        {
            ItineraryStatus.Confirmed => "text-bg-success",
            ItineraryStatus.Completed => "text-bg-primary",
            ItineraryStatus.Cancelled => "text-bg-danger",
            _                         => "text-bg-secondary"
        };

        var legs     = _legs.GetByItinerary(_trip.ItineraryId);
        var requests = _requests.GetByItinerary(_trip.ItineraryId);

        BindLegs(legs, requests);
        BindUnplaced(requests);
        BindTotals(legs, requests);

        gvBookings.DataSource = _itineraries.GetBookingLines(_trip.ItineraryId);
        gvBookings.DataBind();

        pnlAddLeg.Visible = _trip.ClientCanEdit;

        pnlConfirm.Visible = _trip.ClientCanEdit && legs.Count > 0;

        pnlClosed.Visible = !_trip.ClientCanEdit;

        if (pnlClosed.Visible)
            litClosed.Text = _trip.HasPassed
                ? "This trip has been and gone, so it can no longer be changed."
                : _trip.Status == ItineraryStatus.Cancelled
                    ? "This trip was cancelled."
                    : _trip.Status == ItineraryStatus.Confirmed
                        ? "Your agent is looking after this trip now. Contact us to change anything."
                        : "This trip is finished.";
        else if (_trip.Status == ItineraryStatus.Confirmed)
        {
            pnlClosed.Visible = true;
            litClosed.Text = "You have confirmed this trip, so your agent is looking "
                           + "after it now. Contact us to change anything.";
        }

        pnlFirstLegStart.Visible = legs.Count == 0;

        litAddLegHeading.Text = legs.Count == 0 ? "Add your first leg" : "Add the next leg";

        litChainHint.Text = legs.Count == 0
            ? "Tell us when you are leaving and how long you are staying in the first place."
            : "This leg starts on " + legs[legs.Count - 1].EndDate.ToString("dd/MM/yyyy")
              + ", the day your last leg ends.";

        if (legs.Count == 0)
            txtStart.Text = (_trip.StartDate ?? DateTime.Today.AddMonths(1)).ToString("yyyy-MM-dd");
    }

    private void BindLegs(List<TripLeg> legs, List<TripRequestItem> requests)
    {
        lblLegCount.Text = legs.Count.ToString();

        pnlNoLegs.Visible = legs.Count == 0;

        rptLegs.DataSource = legs;
        rptLegs.DataBind();

        for (var i = 0; i < rptLegs.Items.Count; i++)
        {
            var leg = legs[i];

            if (rptLegs.Items[i].FindControl("rptLegActivities") is Repeater inner)
            {
                inner.DataSource = requests
                    .Where(r => r.TripLegId == leg.TripLegId && r.IsAttraction)
                    .ToList();
                inner.DataBind();
            }

            if (rptLegs.Items[i].FindControl("rptLegTransport") is Repeater transport)
            {
                transport.DataSource = requests
                    .Where(r => r.TripLegId == leg.TripLegId && r.IsTransport)
                    .ToList();
                transport.DataBind();
            }
        }
    }

    private void BindUnplaced(List<TripRequestItem> requests)
    {
        var unplaced = requests.Where(r => r.TripLegId == null).ToList();

        pnlUnplaced.Visible = unplaced.Count > 0;

        gvUnplaced.DataSource = unplaced;
        gvUnplaced.DataBind();
    }

    private void BindTotals(List<TripLeg> legs, List<TripRequestItem> requests)
    {
        litBudget.Text = Money(_trip!.TotalBudget);
        litBooked.Text = Money(_trip.BookingsTotal);

        var planned_legs = legs.Sum(l => l.LegExpenses);

        var unplaced = requests.Where(r => r.TripLegId == null && !r.IsDeclined)
                               .Sum(r => r.EstimatedCost);

        var resorts    = planned_legs;
        var activities = unplaced;

        var planned = resorts + activities;

        litEstimate.Text = Money(planned);

        var balance = _trip.TotalBudget - planned;

        litBalanceLabel.Text = balance < 0 ? "Over budget by" : "Running balance";
        lblBalance.Text      = Money(Math.Abs(balance));
        lblBalance.CssClass  = balance < 0 ? "text-danger" : "text-success";

        litDates.Text = legs.Count == 0
            ? "Not set yet"
            : legs[0].StartDate.ToString("dd/MM/yyyy") + " to "
              + legs[legs.Count - 1].EndDate.ToString("dd/MM/yyyy");
    }

    protected void btnAddLeg_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid || _trip == null)
            return;

        var nights = int.TryParse(txtNights.Text.Trim(), out var n) ? n : 1;
        var guests = int.TryParse(txtGuests.Text.Trim(), out var g) ? g : 2;

        int? provinceId = int.TryParse(ddlRegion.SelectedValue, out var pid) ? pid : (int?)null;

        if (provinceId == null)
        {
            ShowMessage("Please choose the region this leg is in.", "alert-danger");
            Show();
            return;
        }

        var existing = _legs.GetByItinerary(_trip.ItineraryId);

        var firstStart = DateTime.Today;

        if (existing.Count == 0)
        {
            var typed = ParseDate(txtStart.Text);

            if (typed == null)
            {
                ShowMessage("Please say when you are leaving.", "alert-danger");
                Show();
                return;
            }

            firstStart = typed.Value;
        }

        _legs.Append(_trip.ItineraryId, nights, firstStart,
                     null, provinceId, guests);

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_trip.ItineraryId}&legadded=1");
    }

    protected void btnConfirmBooking_Click(object sender, EventArgs e)
    {
        if (_trip == null || !_trip.IsEditable || _trip.Status != ItineraryStatus.Draft)
            return;

        _itineraries.SetStatus(_trip.ItineraryId, ItineraryStatus.Confirmed);

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_trip.ItineraryId}&confirmed=1");
    }

    protected void btnCancelBooking_Click(object sender, EventArgs e)
    {
        if (_trip == null || !_trip.IsEditable)
            return;

        _itineraries.SetStatus(_trip.ItineraryId, ItineraryStatus.Cancelled);

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_trip.ItineraryId}&cancelled=1");
    }

    protected void rptLegs_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "RemoveLeg" || _trip == null)
            return;

        var legId = Convert.ToInt32(e.CommandArgument);
        var leg   = _legs.GetById(legId);

        if (leg == null || leg.ItineraryId != _trip.ItineraryId)
        {
            ShowMessage("That leg is not part of this trip.", "alert-danger");
            Show();
            return;
        }

        _legs.Remove(legId);

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_trip.ItineraryId}&legremoved=1");
    }

    protected void gvUnplaced_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "RemoveRequest" || _trip == null)
            return;

        var id = Convert.ToInt32(e.CommandArgument);

        if (_requests.RemoveIfPending(id, _trip.ItineraryId))
            ShowMessage("Removed from your trip.", "alert-success");
        else
            ShowMessage("Your agent has already dealt with that one, so it cannot be removed here.",
                        "alert-warning");

        Show();
    }

    private void ShowWhatJustHappened()
    {
        var q = Request.QueryString;

        if (q["legadded"] == "1")
            ShowMessage("Leg added. Now choose a resort for it and add some activities.",
                        "alert-success");
        else if (q["legremoved"] == "1")
            ShowMessage("Leg removed. The legs after it have moved up to close the gap.",
                        "alert-success");
        else if (q["resort"] == "1")
            ShowMessage("Resort chosen. Your agent will confirm it.", "alert-success");
        else if (q["added"] == "1")
            ShowMessage("Added to your trip. Your agent will confirm it.", "alert-success");
        else if (q["transport"] == "1")
            ShowMessage("Transport added to that leg. Your agent will confirm it.",
                        "alert-success");
        else if (q["confirmed"] == "1")
            ShowMessage("Thank you - your trip is confirmed and your agent has it now.",
                        "alert-success");
        else if (q["cancelled"] == "1")
            ShowMessage("Your trip has been cancelled.", "alert-warning");
    }

    private static string Money(decimal amount) => "R" + amount.ToString("N2");

    private static DateTime? ParseDate(string value)
        => DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out var result)
               ? result
               : (DateTime?)null;

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
