using System.Text;
using System.Web.UI;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Itineraries;

public partial class Gantt : Page
{
    private GanttRepository _gantt => Repositories.Gantt;

    private DateTime Month
    {
        get => ViewState["Month"] as DateTime?
               ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        set => ViewState["Month"] = value;
    }

    private bool ShowingCars
    {
        get => ViewState["ShowingCars"] as bool? ?? false;
        set => ViewState["ShowingCars"] = value;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Draw();
    }

    protected void btnPrev_Click(object sender, EventArgs e)
    {
        Month = Month.AddMonths(-1);
        Draw();
    }

    protected void btnNext_Click(object sender, EventArgs e)
    {
        Month = Month.AddMonths(1);
        Draw();
    }

    protected void btnToday_Click(object sender, EventArgs e)
    {
        Month = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Draw();
    }

    protected void lnkTabResorts_Click(object sender, EventArgs e)
    {
        ShowingCars = false;
        Draw();
    }

    protected void lnkTabCars_Click(object sender, EventArgs e)
    {
        ShowingCars = true;
        Draw();
    }

    private void Draw()
    {
        litMonth.Text = Month.ToString("MMMM yyyy");

        lnkTabResorts.CssClass = ShowingCars ? "nav-link" : "nav-link active";
        lnkTabCars.CssClass    = ShowingCars ? "nav-link active" : "nav-link";

        try
        {
            var bars = ShowingCars
                ? _gantt.GetVehicleBars(Month)
                : _gantt.GetResortBars(Month);

            litChart.Text = BuildTable(bars);
        }
        catch (Exception ex)
        {
            litChart.Text = "";
            ShowMessage("Could not load the chart. " + Server.HtmlEncode(ex.Message),
                        "alert-danger");
        }
    }

    private string BuildTable(List<GanttBar> bars)
    {
        if (bars.Count == 0)
            return "<p class=\"text-muted mb-0\">Nothing to show. There are no "
                 + (ShowingCars ? "vehicles" : "resort units")
                 + " captured yet.</p>";

        var days = DateTime.DaysInMonth(Month.Year, Month.Month);

        var html = new StringBuilder();

        html.Append("<table class=\"gantt\">");

        html.Append("<thead><tr><th class=\"gantt-rowhead\">")
            .Append(ShowingCars ? "Vehicle" : "Unit")
            .Append("</th>");

        for (var day = 1; day <= days; day++)
        {
            var date = new DateTime(Month.Year, Month.Month, day);

            var classes = "gantt-day";

            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                classes += " gantt-weekend";

            if (date.Date == DateTime.Today)
                classes += " gantt-today";

            html.Append("<th class=\"").Append(classes).Append("\">")
                .Append(day).Append("</th>");
        }

        html.Append("</tr></thead><tbody>");

        foreach (var group in bars.GroupBy(b => b.GroupName))
        {
            html.Append("<tr class=\"gantt-group\"><td colspan=\"")
                .Append(days + 1).Append("\">")
                .Append(Server.HtmlEncode(group.Key))
                .Append("</td></tr>");

            foreach (var row in group.GroupBy(b => b.RowId))
            {
                var first = row.First();

                html.Append("<tr><td class=\"gantt-rowhead\">")
                    .Append(Server.HtmlEncode(first.RowName));

                if (!string.IsNullOrWhiteSpace(first.RowDetail))
                    html.Append("<span class=\"text-muted small\"> ")
                        .Append(Server.HtmlEncode(first.RowDetail))
                        .Append("</span>");

                html.Append("</td>");

                for (var day = 1; day <= days; day++)
                {
                    var date = new DateTime(Month.Year, Month.Month, day);

                    var covering = row.Where(b => b.Covers(date)).ToList();

                    var classes = "gantt-cell";

                    if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                        classes += " gantt-weekend";

                    var title = "";

                    if (covering.Count > 1)
                    {
                        classes += " gantt-clash";
                        title = "CLASH: " + string.Join(" | ", covering.Select(b => b.Tooltip));
                    }
                    else if (covering.Count == 1)
                    {
                        classes += covering[0].IsDraft ? " gantt-draft" : " gantt-booked";
                        title = covering[0].Tooltip;
                    }

                    html.Append("<td class=\"").Append(classes).Append("\"");

                    if (title.Length > 0)
                        html.Append(" title=\"").Append(Server.HtmlEncode(title)).Append("\"");

                    html.Append("></td>");
                }

                html.Append("</tr>");
            }
        }

        html.Append("</tbody></table>");

        return html.ToString();
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
