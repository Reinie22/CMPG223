using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Itineraries;

/// <summary>
/// One trip: what it costs, what is booked on it, and the agent's controls for
/// editing and moving it through its statuses.
///
/// WHO MAY OPEN IT
/// ===============
/// The Web.config rule on this folder lets any staff member in. That is not
/// enough on its own, because an agent typing another agent's trip id into the
/// address bar would otherwise be editing somebody else's work. So the page
/// checks ownership as well: an agent gets their own trips, management gets all
/// of them.
///
/// THE STATUSES
/// ============
/// Draft to Confirmed is the agent saying the trip is agreed with the client.
/// Confirmed to Completed is the trip having happened. Cancel goes through
/// ItineraryRepository.Delete, which cancels rather than deletes once anything
/// is booked, so the agency keeps the record of what it sold.
/// </summary>
public partial class EditTrip : Page
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
            FillForm();

        // Always re-evaluated: a status change during this request has to be
        // reflected in which buttons are showing when the page comes back.
        ShowStatus();
        SetButtonAvailability();

        if (!IsPostBack)
            ShowWhatJustHappened();
    }

    /// <summary>
    /// The action handlers finish with a redirect, so the message they want to
    /// show has to survive as a query string flag rather than a field.
    /// </summary>
    private void ShowWhatJustHappened()
    {
        var q = Request.QueryString;

        if (q["saved"] == "1")
            ShowMessage("Trip saved.", "alert-success");
        else if (q["confirmed"] == "1")
            ShowMessage("The trip is confirmed.", "alert-success");
        else if (q["completed"] == "1")
            ShowMessage("The trip is marked as completed.", "alert-success");
        else if (q["cancelled"] == "1")
            ShowMessage("This trip has bookings on it, so it was cancelled rather than deleted.",
                        "alert-warning");
    }

    /// <summary>
    /// Fetches the trip with its totals and checks the user is allowed it.
    /// Returns false when the page has already redirected or shown an error.
    /// </summary>
    private bool LoadTrip()
    {
        if (TripId == null)
        {
            Response.Redirect("~/Itineraries/Default.aspx");
            return false;
        }

        _trip = _itineraries.GetWithTotals(TripId.Value);

        if (_trip == null)
        {
            Response.Redirect("~/Itineraries/Default.aspx");
            return false;
        }

        // An agent may only work on their own trips. Management sees everything.
        var isMine = _trip.AgentId == _user!.UserId;

        if (_user.IsAgent && !isMine)
        {
            pnlTrip.Visible = false;
            ShowMessage("That trip belongs to another agent.", "alert-danger");
            return false;
        }

        return true;
    }

    private void FillForm()
    {
        txtTripName.Text   = _trip!.TripName;
        txtBudget.Text     = _trip.TotalBudget.ToString("0.00", CultureInfo.InvariantCulture);
        txtStartDate.Text  = _trip.StartDate?.ToString("yyyy-MM-dd") ?? "";
        txtEndDate.Text    = _trip.EndDate?.ToString("yyyy-MM-dd") ?? "";
        txtComments.Text   = _trip.TripComments;
    }

    private void ShowStatus()
    {
        litHeading.Text = Server.HtmlEncode(_trip!.TripName);

        litSubtitle.Text = Server.HtmlEncode(
            $"{_trip.ClientName} · {_trip.AgentName ?? "no agent yet"} · {_trip.DateRangeDisplay}");

        lblStatus.Text = _trip.Status;
        lblStatus.CssClass = "badge " + _trip.Status switch
        {
            ItineraryStatus.Confirmed => "text-bg-success",
            ItineraryStatus.Completed => "text-bg-primary",
            ItineraryStatus.Cancelled => "text-bg-danger",
            _                         => "text-bg-secondary"
        };

        litBudgetTotal.Text   = Money(_trip.TotalBudget);
        litBookingsTotal.Text = Money(_trip.BookingsTotal);
        litExpensesTotal.Text = Money(_trip.ExpensesTotal);

        litRemainingLabel.Text = _trip.IsOverBudget ? "Over by" : "Left";
        litRemaining.Text      = Money(Math.Abs(_trip.BudgetRemaining));

        pnlBudgetBar.Attributes["style"] = "width:" + _trip.BudgetUsedPercent + "%";
        pnlBudgetBar.CssClass = _trip.IsOverBudget ? "progress-bar bg-danger" : "progress-bar";
        pnlOverBudget.Visible = _trip.IsOverBudget;

        gvBookings.DataSource = _itineraries.GetBookingLines(_trip.ItineraryId);
        gvBookings.DataBind();

        BindRequests();

        lnkBookTransport.NavigateUrl =
            "~/Itineraries/BookTransport.aspx?itineraryId=" + _trip.ItineraryId;
    }

    /// <summary>
    /// A finished trip is history. Hiding the controls rather than disabling
    /// them keeps the page honest about what can still be done.
    /// </summary>
    private void SetButtonAvailability()
    {
        var editable = _trip!.IsEditable;

        btnSave.Visible          = editable;
        lnkBookTransport.Visible = editable;
        btnCancelTrip.Visible    = editable;

        btnConfirm.Visible  = _trip.Status == ItineraryStatus.Draft;
        btnComplete.Visible = _trip.Status == ItineraryStatus.Confirmed;
    }

    /// <summary>
    /// What the client asked for. The panel stays hidden when they have asked
    /// for nothing, so an agent-built trip is not cluttered by an empty table.
    /// </summary>
    private void BindRequests()
    {
        var requests = _requests.GetByItinerary(_trip!.ItineraryId);

        pnlRequests.Visible = requests.Count > 0;

        var pending = requests.Count(r => r.IsRequested);

        lblPendingCount.Text = pending + " waiting";
        lblPendingCount.Visible = pending > 0;

        gvRequests.DataSource = requests;
        gvRequests.DataBind();
    }

    protected void gvRequests_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (_trip == null)
            return;

        string status;

        if (e.CommandName == "ApproveRequest")
            status = RequestStatus.Approved;
        else if (e.CommandName == "DeclineRequest")
            status = RequestStatus.Declined;
        else
            return;

        var id = Convert.ToInt32(e.CommandArgument);

        // Decide only changes a row that is still 'Requested', so two agents
        // with the page open cannot overwrite each other's decision.
        if (_requests.Decide(id, status, agentNote: null))
            ShowMessage(status == RequestStatus.Approved
                ? "Approved. Now make the real booking on the booking screens."
                : "Declined. The client sees this on their own trip page.",
                status == RequestStatus.Approved ? "alert-success" : "alert-warning");
        else
            ShowMessage("That one had already been decided.", "alert-warning");

        BindRequests();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid || _trip == null)
            return;

        var startDate = ParseDate(txtStartDate.Text);
        var endDate   = ParseDate(txtEndDate.Text);

        // The database has CHECK (EndDate >= StartDate). Saying so here is
        // friendlier than letting SQL Server throw.
        if (startDate != null && endDate != null && endDate < startDate)
        {
            ShowMessage("The end date cannot be before the start date.", "alert-danger");
            return;
        }

        // Everything not on the form is carried over from the loaded trip, so
        // saving never quietly drops the client or the agent off the row.
        _trip.TripName     = txtTripName.Text.Trim();
        _trip.TotalBudget  = decimal.Parse(txtBudget.Text.Trim(), CultureInfo.InvariantCulture);
        _trip.StartDate    = startDate;
        _trip.EndDate      = endDate;
        _trip.TripComments = txtComments.Text.Trim();

        _itineraries.Update(_trip);

        Reload("saved");
    }

    protected void btnConfirm_Click(object sender, EventArgs e)
        => ChangeStatus(ItineraryStatus.Confirmed, "confirmed");

    protected void btnComplete_Click(object sender, EventArgs e)
        => ChangeStatus(ItineraryStatus.Completed, "completed");

    private void ChangeStatus(string status, string flag)
    {
        if (_trip == null)
            return;

        _itineraries.SetStatus(_trip.ItineraryId, status);
        Reload(flag);
    }

    protected void btnCancelTrip_Click(object sender, EventArgs e)
    {
        if (_trip == null)
            return;

        // Delete cancels instead of deleting once anything is booked, and tells
        // us which it did. A deleted trip has no page left to go back to.
        if (_itineraries.Delete(_trip.ItineraryId))
            Reload("cancelled");
        else
            Response.Redirect("~/Itineraries/Default.aspx?deleted=1");
    }

    /// <summary>
    /// Comes back as a fresh GET so the totals, the status badge and the
    /// buttons are all rebuilt from the database rather than from view state.
    /// </summary>
    private void Reload(string flag)
        => Response.Redirect($"~/Itineraries/EditTrip.aspx?id={_trip!.ItineraryId}&{flag}=1");

    private static string Money(decimal amount)
        => "R" + amount.ToString("N2");

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
