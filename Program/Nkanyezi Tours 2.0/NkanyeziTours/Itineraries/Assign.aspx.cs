using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Itineraries;

public partial class Assign : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private UserRepository _users => Repositories.Users;
    private AuthService _auth => Repositories.Auth;

    private sealed class Workload
    {
        public string AgentName { get; set; } = "";
        public int LiveCount { get; set; }
        public int TotalCount { get; set; }
    }

    private List<AppUser> _agents = new List<AppUser>();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (_auth.GetCurrentUser() == null)
        {
            Response.Redirect("~/Account/Login.aspx");
            return;
        }

        _agents = _users.GetTravelAgents();

        if (!IsPostBack)
        {
            BindWorkload();
            BindTrips();
        }
    }

    private void BindWorkload()
    {
        var all = _itineraries.GetAll();

        rptWorkload.DataSource = _agents
            .Select(a => new Workload
            {
                AgentName  = a.FullName,
                LiveCount  = all.Count(t => t.AgentId == a.UserId && t.IsEditable),
                TotalCount = all.Count(t => t.AgentId == a.UserId)
            })
            .ToList();

        rptWorkload.DataBind();
    }

    private void BindTrips()
    {
        var all = _itineraries.GetAll();

        var trips = ddlFilter.SelectedValue switch
        {
            "unassigned" => all.Where(t => t.IsUnassigned && t.IsEditable).ToList(),
            "live"       => all.Where(t => t.IsEditable).ToList(),
            _            => all
        };

        gvTrips.DataSource = trips;
        gvTrips.DataBind();
    }

    protected void gvTrips_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        if (e.Row.FindControl("ddlAgent") is not DropDownList ddl)
            return;

        ddl.Items.Add(new ListItem("Back to the queue", ""));

        foreach (var agent in _agents)
            ddl.Items.Add(new ListItem(agent.FullName, agent.UserId.ToString()));

        var trip = (Itinerary)e.Row.DataItem;

        ddl.SelectedValue = trip.AgentId?.ToString() ?? "";

        if (!trip.IsEditable)
        {
            ddl.Enabled = false;

            if (e.Row.FindControl("lnkAssign") is LinkButton assign)
                assign.Visible = false;
        }
    }

    protected void ddlFilter_Changed(object sender, EventArgs e)
    {
        BindTrips();
    }

    protected void gvTrips_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "AssignTrip")
            return;

        var itineraryId = Convert.ToInt32(e.CommandArgument);

        var row = ((Control)e.CommandSource).NamingContainer as GridViewRow;

        if (row?.FindControl("ddlAgent") is not DropDownList ddl)
            return;

        int? agentId = int.TryParse(ddl.SelectedValue, out var id) ? id : (int?)null;

        _itineraries.AssignToAgent(itineraryId, agentId);

        var trip = _itineraries.GetById(itineraryId);

        ShowMessage(agentId == null
            ? $"\"{Server.HtmlEncode(trip?.TripName)}\" is back in the queue for any agent to pick up."
            : $"\"{Server.HtmlEncode(trip?.TripName)}\" is assigned to {Server.HtmlEncode(trip?.AgentName)}.",
            "alert-success");

        BindWorkload();
        BindTrips();
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
