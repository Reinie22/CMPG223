using System.Globalization;
using System.Web.UI;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

public partial class BookTrip : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private AuthService _auth => Repositories.Auth;

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

        var trip = new Itinerary
        {
            ClientId     = user.UserId,
            AgentId      = null,
            TripName     = txtTripName.Text.Trim(),
            StartDate    = null,
            EndDate      = null,
            TotalBudget  = decimal.Parse(txtBudget.Text.Trim(), CultureInfo.InvariantCulture),
            TripComments = txtComments.Text.Trim(),
            Status       = ItineraryStatus.Draft
        };

        var tripId = _itineraries.Create(trip);

        Response.Redirect($"~/Trips/MyTrip.aspx?id={tripId}&created=1");
    }

    private void ShowError(string text)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 alert-danger";
        pnlMessage.Visible = true;
    }
}
