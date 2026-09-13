using System.Globalization;
using System.Web.UI;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Routes;

public partial class Edit : Page
{
    private RouteRepository _routes => Repositories.Routes;

    private int? RouteId =>
        int.TryParse(Request.QueryString["id"], out var id) ? id : (int?)null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack || RouteId == null)
            return;

        var route = _routes.GetById(RouteId.Value);

        if (route == null)
        {
            Response.Redirect("~/Maintain/Routes/Default.aspx");
            return;
        }

        litHeading.Text       = "Edit " + Server.HtmlEncode(route.Display);
        ddlType.SelectedValue = route.RouteType;
        txtCarrier.Text       = route.Carrier;
        txtCode.Text          = route.RouteCode;
        txtFrom.Text          = route.DepartureCity;
        txtTo.Text            = route.ArrivalCity;
        txtDeparts.Text       = TimeText(route.DepartureTime);
        txtArrives.Text       = TimeText(route.ArrivalTime);
        txtDuration.Text      = route.DurationMinutes?.ToString() ?? "";
        txtSeats.Text         = route.SeatCapacity.ToString();
        txtFare.Text          = route.IndicativeFarePerSeat.ToString("0.00", CultureInfo.InvariantCulture);
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var from = txtFrom.Text.Trim();
        var to   = txtTo.Text.Trim();

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            ShowMessage("The departure and arrival cities cannot be the same.", "alert-danger");
            return;
        }

        var route = new Route
        {
            RouteType             = ddlType.SelectedValue,
            Carrier               = txtCarrier.Text.Trim(),
            RouteCode             = txtCode.Text.Trim(),
            DepartureCity         = from,
            ArrivalCity           = to,
            DepartureTime         = ParseTime(txtDeparts.Text),
            ArrivalTime           = ParseTime(txtArrives.Text),
            DurationMinutes       = int.TryParse(txtDuration.Text.Trim(), out var mins) ? mins : (int?)null,
            SeatCapacity          = int.Parse(txtSeats.Text.Trim()),
            IndicativeFarePerSeat = decimal.Parse(txtFare.Text.Trim(), CultureInfo.InvariantCulture)
        };

        if (RouteId.HasValue)
        {
            route.RouteId = RouteId.Value;
            _routes.Update(route);
        }
        else
        {
            _routes.Create(route);
        }

        Response.Redirect("~/Maintain/Routes/Default.aspx?saved=1");
    }

    private static string TimeText(TimeSpan? value)
        => value == null ? "" : value.Value.ToString(@"hh\:mm");

    private static TimeSpan? ParseTime(string value)
        => TimeSpan.TryParse(value.Trim(), CultureInfo.InvariantCulture, out var result)
               ? result
               : (TimeSpan?)null;

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
