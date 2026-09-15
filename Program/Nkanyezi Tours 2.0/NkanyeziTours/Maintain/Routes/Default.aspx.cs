using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;

namespace NkanyeziTours.Maintain.Routes;

public partial class _Default : Page
{
    private RouteRepository _routes => Repositories.Routes;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        if (Request.QueryString["saved"] == "1")
            ShowMessage("Route saved.", "alert-success");

        BindGrid();
    }

    private void BindGrid()
    {
        var type = ddlType.SelectedValue;
        var from = txtFrom.Text.Trim();
        var to   = txtTo.Text.Trim();

        var hasFilter = type.Length > 0 || from.Length > 0 || to.Length > 0;

        gvRoutes.DataSource = hasFilter
            ? _routes.Search(type, from, to, carrier: null, maxFare: null, includeInactive: true)
            : _routes.GetAll(includeInactive: true);

        gvRoutes.DataBind();
    }

    protected void btnSearch_Click(object sender, EventArgs e) => BindGrid();

    protected void btnClear_Click(object sender, EventArgs e)
    {
        ddlType.SelectedIndex = 0;
        txtFrom.Text = "";
        txtTo.Text   = "";
        BindGrid();
    }

    protected void gvRoutes_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        var routeId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "WithdrawRoute")
        {
            var keptAndWithdrawn = _routes.Delete(routeId);

            ShowMessage(keptAndWithdrawn
                ? "That route is booked or chosen on a client's trip, so it was withdrawn rather than deleted."
                : "Route deleted.",
                keptAndWithdrawn ? "alert-warning" : "alert-success");

            BindGrid();
        }
        else if (e.CommandName == "ReactivateRoute")
        {
            _routes.Reactivate(routeId);
            ShowMessage("Route put back into service.", "alert-success");
            BindGrid();
        }
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
