using System.Globalization;
using System.Web.UI;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

/// <summary>
/// Lets a client start their own trip from the dashboard.
///
/// The trip is saved as a Draft with no agent on it. That is what puts it in
/// the unassigned queue the agents work from - see ItineraryRepository
/// GetUnassigned - so a client request and an agent-built trip are the same
/// kind of row rather than two separate things to reconcile.
/// </summary>
public partial class BookTrip : Page
{
    private readonly ItineraryRepository _itineraries = Repositories.Itineraries;
    private readonly AuthService _auth = Repositories.Auth;

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var user = _auth.GetCurrentUser();

        if (user == null)
        {
            Response.Redirect("~/Account/Login.aspx");
            return;
        }

        var startDate = ParseDate(txtStartDate.Text);
        var endDate   = ParseDate(txtEndDate.Text);

        // The database has CHECK (EndDate >= StartDate). Saying so here is
        // friendlier than letting SQL Server throw.
        if (startDate != null && endDate != null && endDate < startDate)
        {
            ShowError("The return date cannot be before the departure date.");
            return;
        }

        var trip = new Itinerary
        {
            ClientId     = user.UserId,
            AgentId      = null,
            TripName     = txtTripName.Text.Trim(),
            StartDate    = startDate,
            EndDate      = endDate,
            TotalBudget  = decimal.Parse(txtBudget.Text.Trim(), CultureInfo.InvariantCulture),
            TripComments = txtComments.Text.Trim(),
            Status       = ItineraryStatus.Draft
        };

        // Straight to the trip page, so they can start choosing places to stay
        // and things to do instead of hunting for it from the dashboard.
        var tripId = _itineraries.Create(trip);

        Response.Redirect($"~/Trips/MyTrip.aspx?id={tripId}&created=1");
    }

    private static DateTime? ParseDate(string value)
        => DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out var result)
               ? result
               : (DateTime?)null;

    private void ShowError(string text)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 alert-danger";
        pnlMessage.Visible = true;
    }
}
