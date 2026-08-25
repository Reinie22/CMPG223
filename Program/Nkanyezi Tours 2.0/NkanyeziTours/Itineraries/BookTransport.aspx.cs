using System.Web.UI;

namespace NkanyeziTours.Itineraries;

public partial class BookTransport : Page
{
    private readonly ItineraryRepository _itineraries = Repositories.Itineraries;

    private int? ItineraryId =>
        int.TryParse(Request.QueryString["itineraryId"], out var id) ? id : (int?)null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        try
        {
            if (ItineraryId == null)
            {
                ShowError("Choose an itinerary before booking transport.");
                return;
            }

            var trip = _itineraries.GetById(ItineraryId.Value);
            if (trip == null)
            {
                ShowError("That itinerary no longer exists.");
                return;
            }

            if (!trip.IsEditable)
            {
                ShowError("Transport cannot be added to a completed or cancelled itinerary.");
                return;
            }

            litTrip.Text = Server.HtmlEncode($"{trip.TripName} · {trip.ClientName} · {trip.DateRangeDisplay}");

            var id = trip.ItineraryId.ToString();
            lnkCar.NavigateUrl = $"~/Itineraries/BookVehicle.aspx?itineraryId={id}";
            lnkBus.NavigateUrl = $"~/Itineraries/BookRoute.aspx?itineraryId={id}&type=Shuttle";
            lnkFlight.NavigateUrl = $"~/Itineraries/BookRoute.aspx?itineraryId={id}&type=Flight";

            var booked = Request.QueryString["booked"];
            if (!string.IsNullOrWhiteSpace(booked))
            {
                var reference = Request.QueryString["reference"];
                var referenceText = string.IsNullOrWhiteSpace(reference)
                    ? ""
                    : $" <strong>{Server.HtmlEncode(reference)}</strong>";

                ShowMessage("alert-success",
                    $"Your {Server.HtmlEncode(booked)} booking{referenceText} is confirmed.");
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not load transport options. " + Server.HtmlEncode(ex.Message));
        }
    }

    private void ShowError(string message)
    {
        pnlChoices.Visible = false;
        ShowMessage("alert-danger", message);
    }

    private void ShowMessage(string cssClass, string message)
    {
        pnlMessage.CssClass = "alert py-2 " + cssClass;
        litMessage.Text = message;
        pnlMessage.Visible = true;
    }
}
