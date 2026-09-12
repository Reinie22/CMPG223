using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

/// <summary>
/// The client browses accommodation and puts one onto their own draft trip.
///
/// Adding here creates a TripRequestItem, NOT a RoomBooking. No room is held
/// and no price is committed - the agent does that when they approve it. The
/// prices shown are the published nightly rate, which is why the page says so
/// out loud.
///
/// Note what the client is NOT asked for: which room. Rooms differ in price and
/// availability and that is the agent's job to work out. The client says where
/// and when; the agent picks the room.
/// </summary>
public partial class ChooseAccommodation : Page
{
    private readonly AccommodationRepository _accommodation = Repositories.Accommodation;
    private readonly ItineraryRepository _itineraries = Repositories.Itineraries;
    private readonly TripRequestRepository _requests = Repositories.TripRequests;
    private readonly LookupRepository _lookups = Repositories.Lookups;
    private readonly AuthService _auth = Repositories.Auth;

    private AppUser? _user;
    private Itinerary? _trip;

    private int? TripId =>
        int.TryParse(Request.QueryString["tripId"], out var id) ? id : (int?)null;

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

        lnkBack.NavigateUrl = "~/Trips/MyTrip.aspx?id=" + _trip!.ItineraryId;

        if (!IsPostBack)
        {
            litSubtitle.Text = Server.HtmlEncode($"For \"{_trip.TripName}\"");

            // Default the stay to the trip's own dates, because that is almost
            // always what they want and saves typing them twice.
            txtCheckIn.Text  = _trip.StartDate?.ToString("yyyy-MM-dd") ?? "";
            txtCheckOut.Text = _trip.EndDate?.ToString("yyyy-MM-dd") ?? "";

            BindProvinces();
            BindOptions();
        }
    }

    /// <summary>Loads the trip and refuses anybody else's.</summary>
    private bool LoadTrip()
    {
        if (TripId == null)
        {
            Response.Redirect("~/Default.aspx");
            return false;
        }

        if (!_itineraries.BelongsToClient(TripId.Value, _user!.UserId))
        {
            pnlBody.Visible = false;
            ShowMessage("That trip is not yours.", "alert-danger");
            return false;
        }

        _trip = _itineraries.GetById(TripId.Value);

        if (_trip == null)
        {
            Response.Redirect("~/Default.aspx");
            return false;
        }

        if (!_trip.IsEditable)
        {
            pnlBody.Visible = false;
            ShowMessage("This trip is finished, so nothing more can be added to it.", "alert-warning");
            return false;
        }

        return true;
    }

    private void BindProvinces()
    {
        ddlProvince.DataSource = _lookups.GetProvinces();
        ddlProvince.DataTextField = "Name";
        ddlProvince.DataValueField = "Id";
        ddlProvince.DataBind();
        ddlProvince.Items.Insert(0, new ListItem("Anywhere", ""));
    }

    private void BindOptions()
    {
        var name = txtSearchName.Text.Trim();
        int? provinceId = int.TryParse(ddlProvince.SelectedValue, out var pid) ? pid : (int?)null;
        decimal? maxPrice = decimal.TryParse(txtMaxPrice.Text.Trim(), out var price) ? price : (decimal?)null;

        var hasFilter = name.Length > 0 || provinceId.HasValue || maxPrice.HasValue;

        // includeInactive stays false: a client must never be offered something
        // the agency has withdrawn.
        gvOptions.DataSource = hasFilter
            ? _accommodation.Search(name, provinceId, minStarRating: null, maxPrice: maxPrice)
            : _accommodation.GetAll();

        gvOptions.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindOptions();
    }

    protected void gvOptions_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "ChooseItem" || _trip == null)
            return;

        // The date and guest validators are in the Choose group, and the button
        // is too, so this only runs once they pass.
        if (!Page.IsValid)
            return;

        var checkIn  = ParseDate(txtCheckIn.Text);
        var checkOut = ParseDate(txtCheckOut.Text);

        if (checkIn == null || checkOut == null)
        {
            ShowMessage("Please give both a check-in and a check-out date.", "alert-danger");
            return;
        }

        if (checkOut <= checkIn)
        {
            ShowMessage("The check-out date has to be after the check-in date.", "alert-danger");
            return;
        }

        var accommodationId = Convert.ToInt32(e.CommandArgument);

        _requests.Add(new TripRequestItem
        {
            ItineraryId     = _trip.ItineraryId,
            ItemType        = RequestItemTypes.Accommodation,
            AccommodationId = accommodationId,
            StartDate       = checkIn,
            EndDate         = checkOut,
            Guests          = int.TryParse(txtGuests.Text.Trim(), out var guests) ? guests : 1,
            ClientNote      = txtNote.Text.Trim()
        });

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_trip.ItineraryId}&added=1");
    }

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
