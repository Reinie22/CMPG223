using System.Web.UI;

namespace NkanyeziTours;

/// <summary>
/// The master page: the navigation bar and page frame every screen sits inside.
///
/// Its only job is to show the signed-in user's name and hide the menu items
/// their role may not use. Hiding a menu is tidiness, not security - the real
/// protection is the &lt;location&gt; rules in Web.config, which stop somebody
/// typing a URL in directly.
/// </summary>
public partial class SiteMaster : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var signedIn = AuthService.IsSignedIn;

        phSignedIn.Visible  = signedIn;
        phAnonymous.Visible = !signedIn;

        if (signedIn)
        {
            var role = AuthService.CurrentRole;

            litUserName.Text = Server.HtmlEncode(AuthService.CurrentDisplayName);
            litRole.Text     = Server.HtmlEncode(role);

            // A Client sees none of the staff sections.
            var isStaff      = role != RoleNames.Client;
            var isManagement = role == RoleNames.Administrator || role == RoleNames.Supervisor;

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
