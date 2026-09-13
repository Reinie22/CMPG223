using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Portal;

public partial class Discover : Page
{
    private AccommodationRepository _accommodation => Repositories.Accommodation;
    private RoomRepository _rooms => Repositories.Rooms;
    private AttractionRepository _attractions => Repositories.Attractions;
    private VehicleRepository _vehicles => Repositories.Vehicles;
    private RouteRepository _routes => Repositories.Routes;
    private LookupRepository _lookups => Repositories.Lookups;

    private const int TabStay = 0, TabDo = 1, TabGo = 2;
    private const int GoCars = 0, GoFlights = 1, GoBuses = 2;

    private int Tab
    {
        get => ViewState["Tab"] as int? ?? TabStay;
        set => ViewState["Tab"] = value;
    }

    private int GoKind
    {
        get => ViewState["GoKind"] as int? ?? GoCars;
        set => ViewState["GoKind"] = value;
    }

    private int? ResortId
    {
        get => ViewState["ResortId"] as int?;
        set => ViewState["ResortId"] = value;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindLookups();
            Bind();
        }
    }

    private void BindLookups()
    {
        ddlProvince.DataSource = _lookups.GetProvinces();
        ddlProvince.DataTextField = "Name";
        ddlProvince.DataValueField = "Id";
        ddlProvince.DataBind();
        ddlProvince.Items.Insert(0, new ListItem("Anywhere", ""));

        ddlType.DataSource = _lookups.GetAttractionCategories();
        ddlType.DataTextField = "Name";
        ddlType.DataValueField = "Id";
        ddlType.DataBind();
        ddlType.Items.Insert(0, new ListItem("Any type", ""));
    }

    protected void lnkTabStay_Click(object sender, EventArgs e) { Tab = TabStay; ResortId = null; Bind(); }
    protected void lnkTabDo_Click(object sender, EventArgs e)   { Tab = TabDo;   ResortId = null; Bind(); }
    protected void lnkTabGo_Click(object sender, EventArgs e)   { Tab = TabGo;   ResortId = null; Bind(); }

    protected void lnkGoCars_Click(object sender, EventArgs e)    { GoKind = GoCars;    Bind(); }
    protected void lnkGoFlights_Click(object sender, EventArgs e) { GoKind = GoFlights; Bind(); }
    protected void lnkGoBuses_Click(object sender, EventArgs e)   { GoKind = GoBuses;   Bind(); }

    protected void btnSearch_Click(object sender, EventArgs e) => Bind();

    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtSearch.Text = "";
        ddlProvince.SelectedIndex = 0;
        ddlType.SelectedIndex = 0;
        Bind();
    }

    protected void rptStay_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "ViewRooms")
            return;

        if (int.TryParse(Convert.ToString(e.CommandArgument), out var id))
        {
            ResortId = id;
            Bind();
        }
    }

    protected void btnBackToResorts_Click(object sender, EventArgs e)
    {
        ResortId = null;
        Bind();
    }

    private void Bind()
    {
        var viewingRooms = Tab == TabStay && ResortId != null;
        var routesOnly   = Tab == TabGo && GoKind != GoCars;

        lnkTabStay.CssClass = Tab == TabStay ? "nav-link active" : "nav-link";
        lnkTabDo.CssClass   = Tab == TabDo   ? "nav-link active" : "nav-link";
        lnkTabGo.CssClass   = Tab == TabGo   ? "nav-link active" : "nav-link";

        pnlGoKinds.Visible     = Tab == TabGo;
        lnkGoCars.CssClass     = GoKind == GoCars    ? "nav-link active" : "nav-link";
        lnkGoFlights.CssClass  = GoKind == GoFlights ? "nav-link active" : "nav-link";
        lnkGoBuses.CssClass    = GoKind == GoBuses   ? "nav-link active" : "nav-link";

        pnlFilters.Visible        = !viewingRooms;
        pnlProvinceFilter.Visible = !routesOnly;
        pnlTypeFilter.Visible     = Tab == TabDo;

        rptStay.Visible  = Tab == TabStay && !viewingRooms;
        pnlRooms.Visible = viewingRooms;
        rptDo.Visible    = Tab == TabDo;
        rptCars.Visible  = Tab == TabGo && GoKind == GoCars;
        rptGo.Visible    = routesOnly;

        if (viewingRooms)
        {
            BindRooms(ResortId!.Value);
            pnlNone.Visible = false;
            return;
        }

        var name = txtSearch.Text.Trim();
        int? provinceId = int.TryParse(ddlProvince.SelectedValue, out var pid) ? pid : (int?)null;
        int count;

        if (Tab == TabStay)
        {
            var list = name.Length > 0 || provinceId.HasValue
                ? _accommodation.Search(name, provinceId)
                : _accommodation.GetAll();

            count = list.Count;
            rptStay.DataSource = list;
            rptStay.DataBind();
        }
        else if (Tab == TabDo)
        {
            int? typeId = int.TryParse(ddlType.SelectedValue, out var tid) ? tid : (int?)null;

            var list = name.Length > 0 || provinceId.HasValue || typeId.HasValue
                ? _attractions.Search(name, categoryId: typeId, provinceId: provinceId)
                : _attractions.GetAll();

            count = list.Count;
            rptDo.DataSource = list;
            rptDo.DataBind();
        }
        else if (GoKind == GoCars)
        {
            var list = _vehicles.GetAll()
                .Where(v => provinceId == null || v.ProvinceId == provinceId)
                .Where(v => name.Length == 0
                         || Contains(v.Display, name)
                         || Contains(v.Location, name)
                         || Contains(v.ProvinceName, name))
                .ToList();

            count = list.Count;
            rptCars.DataSource = list;
            rptCars.DataBind();
        }
        else
        {
            var type = GoKind == GoFlights ? RouteTypes.Flight : RouteTypes.Shuttle;

            var list = _routes.GetByType(type)
                .Where(r => name.Length == 0
                         || Contains(r.Carrier, name)
                         || Contains(r.Journey, name))
                .ToList();

            count = list.Count;
            rptGo.DataSource = list;
            rptGo.DataBind();
        }

        pnlNone.Visible = count == 0;
    }

    private void BindRooms(int accommodationId)
    {
        var resort = _accommodation.GetById(accommodationId);

        if (resort == null || !resort.IsActive)
        {
            ResortId = null;
            Bind();
            return;
        }

        litRoomsHeading.Text = Server.HtmlEncode(resort.Name);
        litRoomsMeta.Text    = Server.HtmlEncode(
            string.Join(" - ", new[] { resort.Type, resort.Location }
                                   .Where(x => !string.IsNullOrWhiteSpace(x))));

        var rooms = _rooms.GetByAccommodation(accommodationId)
                          .Where(r => r.AvailabilityStatus)
                          .OrderBy(r => r.RoomNumber)
                          .ToList();

        pnlNoRooms.Visible = rooms.Count == 0;
        rptRooms.DataSource = rooms;
        rptRooms.DataBind();
    }

    private static bool Contains(string? text, string term)
        => text != null && text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
}
