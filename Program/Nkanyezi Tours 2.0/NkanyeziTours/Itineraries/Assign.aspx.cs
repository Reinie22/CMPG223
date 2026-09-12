using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Itineraries;

/// <summary>
/// Allocation, for a supervisor or administrator.
///
/// HOW THIS DIFFERS FROM THE QUEUE
/// ===============================
/// On Itineraries/Default.aspx an agent claims a trip for themselves, and
/// ClaimByAgent refuses once somebody holds it. That is right for self-service
/// but it cannot move work between people.
///
/// This screen is the other half: management picks the agent, can take a trip
/// off one agent and give it to another, and can send it back to the queue.
/// It uses AssignToAgent, which overwrites whoever is on the trip.
///
/// The Web.config rule for this page allows Administrator and Supervisor only,
/// and sits ABOVE the folder rule because the first match wins.
/// </summary>
public partial class Assign : Page
{
    private readonly ItineraryRepository _itineraries = Repositories.Itineraries;
    private readonly UserRepository _users = Repositories.Users;
    private readonly AuthService _auth = Repositories.Auth;

    /// <summary>One card in the workload strip.</summary>
    private sealed class Workload
    {
        public string AgentName { get; set; } = "";
        public int LiveCount { get; set; }
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Loaded once per request and reused for every row's dropdown, instead of
    /// a database call per row.
    /// </summary>
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

    /// <summary>
    /// Fills each row's agent dropdown and preselects whoever is on the trip.
    ///
    /// The dropdowns cannot be bound in the markup because the list of agents
    /// comes from the database, and the selected value differs per row.
    /// </summary>
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

        // A finished trip is history. Let it be looked at, not reallocated.
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

        // The dropdown lives in the row the button was clicked in, so the
        // chosen agent has to be read back out of that row.
        var row = ((Control)e.CommandSource).NamingContainer as GridViewRow;

        if (row?.FindControl("ddlAgent") is not DropDownList ddl)
            return;

        int? agentId = int.TryParse(ddl.SelectedValue, out var id) ? id : (int?)null;

        _itineraries.AssignToAgent(itineraryId, agentId);

        var trip = _itineraries.GetById(itineraryId);

        ShowMessage(agentId == null
            ? $"\"{Server.HtmlEncode(trip?.TripName)}\" is back in the queue for any agent to pick up."
            : $"\"{Server.HtmlEncode(trip?.TripName)}\" is now with {Server.HtmlEncode(trip?.AgentName)}.",
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
