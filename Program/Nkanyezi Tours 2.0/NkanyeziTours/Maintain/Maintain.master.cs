using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain;

public partial class MaintainMaster : MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        pnlAdminOnly.Visible = AuthService.IsInRole(RoleNames.Administrator);

        HighlightCurrentSection();
    }

    private void HighlightCurrentSection()
    {
        var path = Request.AppRelativeCurrentExecutionFilePath ?? "";

        if (Has(path, "/Maintain/Accommodation/"))
        {
            MarkActive(lnkAccommodation);
            pnlRoomsHere.Visible = Has(path, "/Rooms.aspx");
        }
        else if (Has(path, "/Maintain/Attractions/"))
            MarkActive(lnkAttractions);
        else if (Has(path, "/Maintain/Vehicles/Maintenance.aspx"))
            MarkActive(lnkMaintenance);
        else if (Has(path, "/Maintain/Vehicles/"))
            MarkActive(lnkVehicles);
        else if (Has(path, "/Maintain/Routes/"))
            MarkActive(lnkRoutes);
        else if (Has(path, "/Maintain/Users/"))
            MarkActive(lnkUsers);
    }

    private static bool Has(string path, string fragment)
        => path.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;

    private static void MarkActive(HyperLink link)
    {
        link.CssClass += " active fw-semibold";
        link.Attributes["aria-current"] = "page";
    }
}
