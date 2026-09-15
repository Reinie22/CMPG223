using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

public partial class ChooseAttraction : Page
{
    private AttractionRepository _attractions => Repositories.Attractions;
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private TripRequestRepository _requests => Repositories.TripRequests;
    private TripLegRepository _legs => Repositories.TripLegs;
    private LookupRepository _lookups => Repositories.Lookups;
    private AuthService _auth => Repositories.Auth;

    private AppUser? _user;
    private TripLeg? _leg;

    private int? TripLegId =>
        int.TryParse(Request.QueryString["tripLegId"], out var id) ? id : (int?)null;

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

        lnkBack.NavigateUrl = "~/Trips/MyTrip.aspx?id=" + _leg!.ItineraryId;

        if (!IsPostBack)
        {
            litSubtitle.Text = Server.HtmlEncode(
                $"For {_leg.Title} - {_leg.DateDisplay}");

            txtPeople.Text = _leg.Guests.ToString();

            BindDays();
            BindLookups();
            BindOptions();
        }
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
            ShowMessage("This trip has been submitted or has ended, so it can no longer be changed. Contact your agent if something needs to change.", "alert-warning");
            return false;
        }

        return true;
    }

    private void BindDays()
    {
        ddlVisitDate.Items.Clear();

        for (var day = _leg!.StartDate; day < _leg.EndDate; day = day.AddDays(1))
        {
            var value = day.ToString("yyyy-MM-dd");
            ddlVisitDate.Items.Add(new ListItem(day.ToString("ddd dd/MM/yyyy"), value));
        }
    }

    private void BindLookups()
    {
        ddlCategory.DataSource = _lookups.GetAttractionCategories();
        ddlCategory.DataTextField = "Name";
        ddlCategory.DataValueField = "Id";
        ddlCategory.DataBind();
        ddlCategory.Items.Insert(0, new ListItem("Anything", ""));
    }

    private void BindOptions()
    {
        var name = txtSearchName.Text.Trim();
        int? categoryId = int.TryParse(ddlCategory.SelectedValue, out var cid) ? cid : (int?)null;

        var region = _leg!.ProvinceId;

        litRegionNote.Text = region == null
            ? "This leg has no region yet, so nothing can be offered. Set the "
              + "region of the leg on your trip page first."
            : Server.HtmlEncode($"Things to do in {_leg.ProvinceName}. A leg happens "
                                + "in one province, so only that province is offered.");

        if (region == null)
        {
            rptOptions.DataSource = null;
            rptOptions.DataBind();
            pnlNone.Visible = false;
            return;
        }

        var options = _attractions.Search(name, categoryId, region);

        pnlNone.Visible = options.Count == 0;

        rptOptions.DataSource = options;
        rptOptions.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindOptions();
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtSearchName.Text = "";
        ddlCategory.SelectedIndex = 0;
        BindOptions();
    }

    protected void rptOptions_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "ChooseItem" || _leg == null || !pnlBody.Visible)
            return;

        if (!Page.IsValid)
            return;

        var visitDate = ParseDate(ddlVisitDate.SelectedValue);

        if (visitDate == null)
        {
            ShowMessage("Please choose which day.", "alert-danger");
            return;
        }

        var attractionId = Convert.ToInt32(e.CommandArgument);

        _requests.Add(new TripRequestItem
        {
            ItineraryId  = _leg.ItineraryId,
            TripLegId    = _leg.TripLegId,
            ItemType     = RequestItemTypes.Attraction,
            AttractionId = attractionId,
            StartDate    = visitDate,
            EndDate      = null,
            Guests       = int.TryParse(txtPeople.Text.Trim(), out var people) ? people : 1,
            ClientNote   = null
        });

        Response.Redirect($"~/Trips/MyTrip.aspx?id={_leg.ItineraryId}&added=1");
    }

    private static DateTime? ParseDate(string value)
        => DateTime.TryParseExact((value ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
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
