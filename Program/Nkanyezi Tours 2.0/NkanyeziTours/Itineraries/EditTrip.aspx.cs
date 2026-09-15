using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Itineraries;

public partial class EditTrip : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private TripRequestRepository _requests => Repositories.TripRequests;
    private TripLegRepository _legs => Repositories.TripLegs;
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
            FillForm();

        ShowStatus();
        SetButtonAvailability();

        if (!IsPostBack)
            ShowWhatJustHappened();
    }

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

        var isMine = _trip.AgentId == _user!.UserId;

        if (_user.IsAgent && !isMine)
        {
            pnlTrip.Visible = false;

            ShowMessage(_trip.AgentId == null
                            ? "That trip has not been assigned yet. A supervisor decides "
                              + "who takes it."
                            : "That trip belongs to another agent.",
                        "alert-danger");
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

        var requests = _requests.GetByItinerary(_trip.ItineraryId);
        var legs     = _legs.GetByItinerary(_trip.ItineraryId);
        var rooms    = legs.Where(l => l.HasResort && l.IsResortApproved).Sum(l => l.EstimatedResortCost);
        var approved = requests.Where(r => r.IsApproved).Sum(r => r.EstimatedCost);

        _trip.BookingsTotal += rooms + approved;

        litBudgetTotal.Text   = Money(_trip.TotalBudget);
        litBookingsTotal.Text = Money(_trip.BookingsTotal);
        litExpensesTotal.Text = Money(_trip.ExpensesTotal);

        litRemainingLabel.Text = _trip.IsOverBudget ? "Over by" : "Left";
        litRemaining.Text      = Money(Math.Abs(_trip.BudgetRemaining));

        pnlBudgetBar.Attributes["style"] = "width:" + _trip.BudgetUsedPercent + "%";
        pnlBudgetBar.CssClass = _trip.IsOverBudget ? "progress-bar bg-danger" : "progress-bar";
        pnlOverBudget.Visible = _trip.IsOverBudget;

        BindRequests(requests, legs.Where(l => l.HasResort).ToList());

        lnkBookTransport.NavigateUrl =
            "~/Itineraries/BookTransport.aspx?itineraryId=" + _trip.ItineraryId;
    }

    private void SetButtonAvailability()
    {
        var editable = _trip!.IsEditable;

        btnSave.Visible          = editable;
        lnkBookTransport.Visible = editable;
        btnCancelTrip.Visible    = editable;

        btnConfirm.Visible  = _trip.Status == ItineraryStatus.Draft;
        btnComplete.Visible = _trip.Status == ItineraryStatus.Confirmed;
    }

    private void BindRequests(List<TripRequestItem> requests, List<TripLeg> resorts)
    {
        pnlRequests.Visible = requests.Count > 0 || resorts.Count > 0;

        pnlResorts.Visible = resorts.Count > 0;
        gvResorts.DataSource = resorts;
        gvResorts.DataBind();

        var pending = requests.Count(r => r.IsRequested) + resorts.Count(l => l.IsResortRequested);

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

        if (_requests.Decide(id, status, agentNote: null))
            ShowMessage(status == RequestStatus.Approved
                ? "Approved. Its cost has been added to the trip amount."
                : "Declined. The client sees this on their own trip page.",
                status == RequestStatus.Approved ? "alert-success" : "alert-warning");
        else
            ShowMessage("That one had already been decided.", "alert-warning");

        _trip = _itineraries.GetWithTotals(_trip.ItineraryId) ?? _trip;
        ShowStatus();
    }

    protected void gvResorts_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (_trip == null)
            return;

        string status;

        if (e.CommandName == "ApproveResort")
            status = RequestStatus.Approved;
        else if (e.CommandName == "DeclineResort")
            status = RequestStatus.Declined;
        else
            return;

        var legId = Convert.ToInt32(e.CommandArgument);

        if (_legs.DecideResort(legId, _trip.ItineraryId, status))
            ShowMessage(status == RequestStatus.Approved
                ? "Accommodation approved. Its cost has been added to the trip amount."
                : "Accommodation declined. The client sees this on their own trip page.",
                status == RequestStatus.Approved ? "alert-success" : "alert-warning");
        else
            ShowMessage("That one had already been decided.", "alert-warning");

        _trip = _itineraries.GetWithTotals(_trip.ItineraryId) ?? _trip;
        ShowStatus();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid || _trip == null)
            return;

        var startDate = ParseDate(txtStartDate.Text);
        var endDate   = ParseDate(txtEndDate.Text);

        if (startDate != null && endDate != null && endDate < startDate)
        {
            ShowMessage("The end date cannot be before the start date.", "alert-danger");
            return;
        }

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

        if (_itineraries.Delete(_trip.ItineraryId))
            Reload("cancelled");
        else
            Response.Redirect("~/Itineraries/Default.aspx?deleted=1");
    }

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
