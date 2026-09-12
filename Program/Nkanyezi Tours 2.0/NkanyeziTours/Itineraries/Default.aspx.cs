using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Itineraries;

/// <summary>
/// The trips screen, and the queue agents pick work up from.
///
/// A client who books a trip from their dashboard creates it with no agent on
/// it. That is what lands it in the queue here - see
/// ItineraryRepository.GetUnassigned. Claiming one writes the agent's id onto
/// the row, which takes it out of the queue and into their own list.
///
/// Only a Travel Agent sees the Claim button. Claiming is self-assignment, and
/// a Supervisor or Administrator putting their own name on a trip is not the
/// same thing as allocating it to somebody - they get the queue read-only.
/// </summary>
public partial class _Default : Page
{
    private readonly ItineraryRepository _itineraries = Repositories.Itineraries;
    private readonly AuthService _auth = Repositories.Auth;

    private AppUser? _user;

    /// <summary>Read by the Claim button's Visible in the markup.</summary>
    protected bool CanClaim => _user != null && _user.IsAgent;

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

            // EditTrip redirects here when a cancel turned out to be a delete,
            // because the page it was on no longer exists.
            if (Request.QueryString["deleted"] == "1")
                ShowMessage("Nothing was booked on that trip, so it was deleted.", "alert-success");
        }
    }

    private void BindEverything()
    {
        litSubtitle.Text = CanClaim
            ? "Pick up a trip from the queue, or carry on with one of yours"
            : "Every trip in the agency, and what is still waiting for an agent";

        litMineHeading.Text = CanClaim ? "My trips" : "All trips";

        // Only management allocates work; agents self-serve from the queue.
        lnkAssign.Visible = !CanClaim;

        BindQueue();

        var mine = CanClaim
            ? _itineraries.GetForAgent(_user!.UserId)
            : _itineraries.GetAll();

        gvMine.DataSource = mine;
        gvMine.DataBind();
    }

    private void BindQueue()
    {
        var waiting = _itineraries.GetUnassigned();

        lblQueueCount.Text = waiting.Count.ToString();

        gvUnassigned.DataSource = waiting;
        gvUnassigned.DataBind();
    }

    protected void gvUnassigned_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "ClaimTrip")
            return;

        if (!CanClaim)
        {
            ShowMessage("Only a travel agent can pick up a trip.", "alert-danger");
            return;
        }

        var itineraryId = Convert.ToInt32(e.CommandArgument);

        // ClaimByAgent only updates the row while AgentId is still empty, so if
        // two agents click at the same moment the second one changes nothing
        // and gets told, rather than quietly taking the trip off the first.
        if (_itineraries.ClaimByAgent(itineraryId, _user!.UserId))
            ShowMessage("The trip is yours - it is now in your list below.", "alert-success");
        else
            ShowMessage("Another agent picked that trip up first.", "alert-warning");

        BindEverything();
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
