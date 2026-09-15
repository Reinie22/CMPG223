using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Itineraries;

public partial class _Default : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private AuthService _auth => Repositories.Auth;

    private AppUser? _user;

    private bool IsAgent => _user != null && _user.IsAgent;

    protected void Page_Load(object sender, EventArgs e)
    {
        _user = _auth.GetCurrentUser();

        if (_user == null)
        {
            Response.Redirect("~/Account/Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            BindEverything();

            if (Request.QueryString["deleted"] == "1")
                ShowMessage("Nothing was booked on that trip, so it was deleted.", "alert-success");
        }
    }

    private void BindEverything()
    {
        litSubtitle.Text = IsAgent
            ? "The trips your supervisor has given you"
            : "Every trip in the agency, and what is still waiting for an agent";

        litMineHeading.Text = IsAgent ? "My trips" : "All trips";

        lnkAssign.Visible = !IsAgent;
        pnlQueue.Visible  = !IsAgent;

        if (!IsAgent)
            BindQueue();

        BindTrips();
    }

    private void BindTrips()
    {
        IEnumerable<Itinerary> trips = IsAgent
            ? _itineraries.GetForAgent(_user!.UserId)
            : _itineraries.GetAll();

        var text = txtSearch.Text.Trim();
        if (text.Length > 0)
            trips = trips.Where(t => t.TripName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0
                                  || (t.ClientName ?? "").IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);

        if (ddlStatus.SelectedValue.Length > 0)
            trips = trips.Where(t => t.Status == ddlStatus.SelectedValue);

        gvMine.DataSource = trips.ToList();
        gvMine.DataBind();
    }

    protected void btnFilter_Click(object sender, EventArgs e)
    {
        BindTrips();
    }

    protected void btnClearFilter_Click(object sender, EventArgs e)
    {
        txtSearch.Text = "";
        ddlStatus.SelectedIndex = 0;
        BindTrips();
    }

    private void BindQueue()
    {
        var waiting = _itineraries.GetUnassigned();

        lblQueueCount.Text = waiting.Count.ToString();

        gvUnassigned.DataSource = waiting;
        gvUnassigned.DataBind();
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
