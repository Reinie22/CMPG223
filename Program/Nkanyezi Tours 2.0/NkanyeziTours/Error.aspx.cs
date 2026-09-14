using System.Web.UI;

namespace NkanyeziTours;

public partial class ErrorPage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var notFound = AppRelativeVirtualPath.EndsWith("NotFound.aspx", StringComparison.OrdinalIgnoreCase);

        if (notFound)
        {
            Title           = "Page not found - Nkanyezi Tours";
            litHeading.Text = "We could not find that page";
            litMessage.Text = "The link may be old or typed incorrectly. Use the buttons below to carry on.";
            Response.StatusCode = 404;
        }
        else
        {
            Title           = "Something went wrong - Nkanyezi Tours";
            litHeading.Text = "Something went wrong";
            litMessage.Text = "Sorry, we could not finish that just now. Please go back and try again. If it keeps happening, let your administrator know.";
            Response.StatusCode = 500;
        }

        Response.TrySkipIisCustomErrors = true;
    }
}
