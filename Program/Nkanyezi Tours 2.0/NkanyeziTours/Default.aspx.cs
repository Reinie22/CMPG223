using System.Web.UI;

namespace NkanyeziTours;

public partial class _Default : Page
{
    private ItineraryRepository _itineraries => Repositories.Itineraries;
    private AuthService _auth => Repositories.Auth;

    private sealed class Stat
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        var root = (Request.ApplicationPath ?? "/").TrimEnd('/') + "/";
        if (string.Equals(Request.RawUrl.Split('?')[0], root, StringComparison.OrdinalIgnoreCase))
        {
            Response.Redirect("~/Portal/Discover.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            return;
        }

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
            {
                Response.Redirect("~/Itineraries/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            else
                LoadManagementView();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void LoadClientView(AppUser user)
    {
        Page.Title = "Itineraries";
        litSubtitle.Text = "Your trips with Nkanyezi Tours";
        litTripsHeading.Text = "My trips";
        pnlBookTrip.Visible = true;

        var trips = _itineraries.GetForClient(user.UserId);

        BindStats(new[]
        {
            new Stat { Label = "Total trips", Value = trips.Count.ToString() },
            new Stat { Label = "Active",   Value = trips.Count(t => t.IsEditable).ToString() }
        });

        BindTrips(trips);
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
        gvTrips.DataSource = trips.Take(10).ToList();
        gvTrips.DataBind();
    }

    private void ShowError(Exception ex)
    {
        litError.Text = "Could not load the dashboard. Check that SQL Server is running and that "
                      + "the database has been created by running 01-schema.sql and 03-seed-data.sql."
                      + "<br /><small class=\"text-muted\">" + Server.HtmlEncode(RealMessage(ex)) + "</small>";
        pnlError.Visible = true;
    }

    private static string RealMessage(Exception ex)
    {
        while (ex is TypeInitializationException && ex.InnerException != null)
            ex = ex.InnerException;

        return ex.Message;
    }
}
