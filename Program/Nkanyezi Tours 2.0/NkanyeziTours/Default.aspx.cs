using System.Web.UI;

namespace NkanyeziTours;

/// <summary>
/// The dashboard: the landing page after signing in.
///
/// The four roles have almost nothing in common, so this page decides what to
/// show based on the signed-in role rather than trying to be one thing for
/// everybody.
/// </summary>
public partial class _Default : Page
{
    private readonly ItineraryRepository _itineraries = Repositories.Itineraries;
    private readonly AuthService _auth = Repositories.Auth;

    /// <summary>A stat card on the dashboard.</summary>
    private sealed class Stat
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        try
        {
            var user = _auth.GetCurrentUser();

            if (user == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            litName.Text = Server.HtmlEncode(user.FullName);
            pnlRequested.Visible = Request.QueryString["requested"] == "1";

            if (user.IsClient)
                LoadClientView(user);
            else if (user.IsAgent)
                LoadAgentView(user);
            else
                LoadManagementView();
        }
        catch (Exception ex)
        {
            // A missing database is the most likely problem on a new machine,
            // so say so on the page rather than showing a yellow crash screen.
            ShowError(ex);
        }
    }

    private void LoadClientView(AppUser user)
    {
        litSubtitle.Text = "Your trips with Nkanyezi Tours";
        pnlBookTrip.Visible = true;

        var trips = _itineraries.GetForClient(user.UserId);

        BindStats(new[]
        {
            new Stat { Label = "My trips", Value = trips.Count.ToString() },
            new Stat { Label = "Active",   Value = trips.Count(t => t.IsEditable).ToString() }
        });

        BindTrips(trips);
    }

    private void LoadAgentView(AppUser user)
    {
        litSubtitle.Text = "Trips assigned to you, and the queue waiting to be picked up";

        var mine       = _itineraries.GetForAgent(user.UserId);
        var unassigned = _itineraries.GetUnassigned();

        BindStats(new[]
        {
            new Stat { Label = "My trips",   Value = mine.Count.ToString() },
            new Stat { Label = "Unassigned", Value = unassigned.Count.ToString() },
            new Stat { Label = "Active",     Value = mine.Count(t => t.IsEditable).ToString() }
        });

        BindTrips(mine);
    }

    private void LoadManagementView()
    {
        litSubtitle.Text = "Agency overview";

        var all = _itineraries.GetAll();

        BindStats(new[]
        {
            new Stat { Label = "Total trips", Value = all.Count.ToString() },
            new Stat { Label = "Confirmed",   Value = all.Count(t => t.Status == ItineraryStatus.Confirmed).ToString() },
            new Stat { Label = "Draft",       Value = all.Count(t => t.Status == ItineraryStatus.Draft).ToString() },
            new Stat { Label = "Unassigned",  Value = all.Count(t => t.IsUnassigned).ToString() }
        });

        // Allocation is management's job, so the shortcut to it lives here. The
        // badge only appears when there is actually something waiting.
        pnlManage.Visible = true;

        var waiting = all.Count(t => t.IsUnassigned && t.IsEditable);

        if (waiting > 0)
        {
            lblUnassignedCount.Text = waiting == 1
                ? "1 trip needs an agent"
                : waiting + " trips need an agent";
            lblUnassignedCount.Visible = true;
        }

        BindTrips(all);
    }

    private void BindStats(IEnumerable<Stat> stats)
    {
        rptStats.DataSource = stats;
        rptStats.DataBind();
    }

    private void BindTrips(List<Itinerary> trips)
    {
        // Newest ten. The full list lives on the itineraries screen.
        gvTrips.DataSource = trips.Take(10).ToList();
        gvTrips.DataBind();
    }

    private void ShowError(Exception ex)
    {
        litError.Text = "Could not load the dashboard. Check that SQL Server is running and that "
                      + "the database has been created by running 01-schema.sql and 03-seed-data.sql."
                      + "<br /><small class=\"text-muted\">" + Server.HtmlEncode(ex.Message) + "</small>";
        pnlError.Visible = true;
    }
}
