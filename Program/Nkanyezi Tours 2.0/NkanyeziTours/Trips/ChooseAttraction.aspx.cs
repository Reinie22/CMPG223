using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

/// <summary>
/// The client browses attractions and puts one onto their own draft trip.
///
/// The same shape as ChooseAccommodation: adding creates a TripRequestItem, not
/// an AttractionBooking, so no slot capacity is held and no price is committed.
/// The rate shown is the published one per person.
/// </summary>
public partial class ChooseAttraction : Page
{
    private readonly AttractionRepository _attractions = Repositories.Attractions;
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

            txtVisitDate.Text = _trip.StartDate?.ToString("yyyy-MM-dd") ?? "";

            BindLookups();
            BindOptions();
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

    private void BindLookups()
    {
        ddlCategory.DataSource = _lookups.GetAttractionCategories();
        ddlCategory.DataTextField = "Name";
        ddlCategory.DataValueField = "Id";
        ddlCategory.DataBind();
        ddlCategory.Items.Insert(0, new ListItem("Anything", ""));

        ddlProvince.DataSource = _lookups.GetProvinces();
        ddlProvince.DataTextField = "Name";
        ddlProvince.DataValueField = "Id";
        ddlProvince.DataBind();
        ddlProvince.Items.Insert(0, new ListItem("Anywhere", ""));
    }

    private void BindOptions()
    {
        var name = txtSearchName.Text.Trim();
        int? categoryId = int.TryParse(ddlCategory.SelectedValue, out var cid) ? cid : (int?)null;
        int? provinceId = int.TryParse(ddlProvince.SelectedValue, out var pid) ? pid : (int?)null;

        var hasFilter = name.Length > 0 || categoryId.HasValue || provinceId.HasValue;

        // Inactive attractions are never offered to a client.
        gvOptions.DataSource = hasFilter
            ? _attractions.Search(name, categoryId, provinceId)
            : _attractions.GetAll();

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

        if (!Page.IsValid)
            return;

        var visitDate = ParseDate(txtVisitDate.Text);

        if (visitDate == null)
        {
            ShowMessage("Please give a date for the visit.", "alert-danger");
            return;
        }

        var attractionId = Convert.ToInt32(e.CommandArgument);

        _requests.Add(new TripRequestItem
        {
            ItineraryId  = _trip.ItineraryId,
            ItemType     = RequestItemTypes.Attraction,
            AttractionId = attractionId,
            StartDate    = visitDate,
            EndDate      = null,
            Guests       = int.TryParse(txtPeople.Text.Trim(), out var people) ? people : 1,
            ClientNote   = txtNote.Text.Trim()
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
