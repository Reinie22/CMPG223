using System.Web.UI;

namespace NkanyeziTours;

public partial class SiteMaster : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var signedIn = AuthService.IsSignedIn;

        phBypassWarning.Visible = AuthService.BypassLogin;

        if (AuthService.BypassLogin)
            litBypassUser.Text = Server.HtmlEncode(AuthService.CurrentDisplayName);

        phSignedIn.Visible  = signedIn;
        phAnonymous.Visible = !signedIn;

        if (signedIn)
        {
            var role = AuthService.CurrentRole;

            litUserName.Text = Server.HtmlEncode(AuthService.CurrentDisplayName);
            litRole.Text     = Server.HtmlEncode(role);

            var isStaff      = role != RoleNames.Client;
            var isManagement = role == RoleNames.Administrator || role == RoleNames.Supervisor;

            litDashboardLabel.Text = isStaff ? "Dashboard" : "Itineraries";

            navMaintain.Visible    = isStaff;
            navItineraries.Visible = isStaff;
            navReports.Visible     = isManagement;
        }
        else
        {
            navMaintain.Visible    = false;
            navItineraries.Visible = false;
            navReports.Visible     = false;
        }
    }

    protected void btnSignOut_Click(object sender, EventArgs e)
    {
        AuthService.Logout();
        Response.Redirect("~/Account/Login.aspx");
    }
}
