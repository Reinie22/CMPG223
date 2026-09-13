using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Trips;

public partial class ChooseAccommodation : Page
{
    private AccommodationRepository _accommodation => Repositories.Accommodation;
    private ItineraryRepository _itineraries => Repositories.Itineraries;
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
                $"For {_leg.Title} - {_leg.DateDisplay}, {_leg.Guests} guests");

            BindProvinces();
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
            ShowMessage("This trip is finished, so it cannot be changed.", "alert-warning");
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

        var options = hasFilter
            ? _accommodation.Search(name, provinceId, minStarRating: null, maxPrice: maxPrice)
            : _accommodation.GetAll();

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
        ddlProvince.SelectedIndex = 0;
        txtMaxPrice.Text = "";
        BindOptions();
    }

    protected void rptOptions_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "ChooseResort" || _leg == null)
            return;

        var accommodationId = Convert.ToInt32(e.CommandArgument);

        _legs.SetResort(_leg.TripLegId, accommodationId);

        Response.Redirect($"~/Trips/ChooseUnit.aspx?tripLegId={_leg.TripLegId}");
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
