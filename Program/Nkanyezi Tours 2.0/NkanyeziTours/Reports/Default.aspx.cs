using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Models;

namespace NkanyeziTours.Reports;

public partial class _Default : Page
{
    private const string FriendlyFailure =
        "The report could not be created just now. Please try again in a moment. If it keeps happening, let your administrator know.";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack)
            return;

        var year = DateTime.Today.Year;
        txtFrom.Text = new DateTime(year, 1, 1).ToString("yyyy-MM-dd");
        txtTo.Text   = new DateTime(year, 12, 31).ToString("yyyy-MM-dd");

        var requested = Request.QueryString["report"];
        if (requested != null && ddlReport.Items.FindByValue(requested) != null)
            ddlReport.SelectedValue = requested;

        LoadClients();
        ShowOptionsForReport();
    }

    protected void ddlReport_SelectedIndexChanged(object sender, EventArgs e)
    {
        pnlReport.Visible  = false;
        pnlMessage.Visible = false;
        ShowOptionsForReport();
    }

    protected void btnShow_Click(object sender, EventArgs e)
    {
        var report = TryBuild();
        if (report == null)
            return;

        Render(report);
    }

    protected void btnExcel_Click(object sender, EventArgs e)
        => Download(ReportExcelWriter.Write,
                    "xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    protected void btnPdf_Click(object sender, EventArgs e)
        => Download(ReportPdfWriter.Write, "pdf", "application/pdf");

    protected void rptSections_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        var section = (ReportSection)e.Item.DataItem;
        var grid    = (GridView)e.Item.FindControl("gvSection");

        grid.EmptyDataText = section.EmptyText;
        grid.EmptyDataRowStyle.CssClass = "text-muted-empty";

        grid.RowDataBound += (s, args) =>
        {
            for (var c = 0; c < section.Columns.Count && c < args.Row.Cells.Count; c++)
                if (section.Columns[c].IsNumeric)
                    args.Row.Cells[c].CssClass = "text-end text-nowrap";

            if (args.Row.RowType == DataControlRowType.DataRow
                && section.TotalRow != null
                && args.Row.RowIndex == section.Rows.Count)
                args.Row.CssClass = "report-total";
        };

        grid.DataSource = ToTable(section);
        grid.DataBind();

        if (grid.HeaderRow != null)
            grid.HeaderRow.TableSection = TableRowSection.TableHeader;
    }

    private void Download(Func<ReportDocument, byte[]> write, string extension, string contentType)
    {
        var report = TryBuild();
        if (report == null)
            return;

        byte[] file;
        try
        {
            file = write(report);
        }
        catch (Exception)
        {
            ShowMessage(FriendlyFailure);
            return;
        }

        Response.Clear();
        Response.ContentType = contentType;
        Response.AddHeader("Content-Disposition", "attachment; filename=\"" + report.FileName(extension) + "\"");
        Response.AddHeader("Content-Length", file.Length.ToString(CultureInfo.InvariantCulture));
        Response.BinaryWrite(file);
        Response.Flush();
        Response.SuppressContent = true;
        Context.ApplicationInstance.CompleteRequest();
    }

    private ReportDocument? TryBuild()
    {
        pnlMessage.Visible = false;

        if (!TryReadDate(txtFrom.Text, out var fromDate) || !TryReadDate(txtTo.Text, out var toDate))
        {
            ShowMessage("Please choose both a From date and a To date.");
            return null;
        }

        if (fromDate > toDate)
        {
            ShowMessage("The From date must be on or before the To date.");
            return null;
        }

        if (toDate > fromDate.AddYears(5))
        {
            ShowMessage("Please choose a period of five years or less.");
            return null;
        }

        int? clientId   = null;
        string? client  = null;

        if (ddlReport.SelectedValue == ReportTypes.TripCost
            && int.TryParse(ddlClient.SelectedValue, out var chosen) && chosen > 0)
        {
            clientId = chosen;
            client   = ddlClient.SelectedItem.Text;
        }

        try
        {
            return new ReportBuilder(Repositories.Reports)
                .Build(ddlReport.SelectedValue, fromDate, toDate, clientId, client, AuthService.CurrentDisplayName);
        }
        catch (Exception)
        {
            ShowMessage(FriendlyFailure);
            return null;
        }
    }

    private void Render(ReportDocument report)
    {
        litTitle.Text       = Server.HtmlEncode(report.Title);
        litDescription.Text = Server.HtmlEncode(report.Description);
        litGenerated.Text   = Server.HtmlEncode(report.GeneratedDisplay);

        rptFilters.DataSource = report.Filters;
        rptFilters.DataBind();

        rptStats.DataSource = report.Stats;
        rptStats.DataBind();

        rptNav.DataSource = report.Sections;
        rptNav.DataBind();

        rptSections.DataSource = report.Sections;
        rptSections.DataBind();

        rptLegend.DataSource = report.Legend;
        rptLegend.DataBind();

        pnlReport.Visible = true;
    }

    private void LoadClients()
    {
        ddlClient.Items.Clear();
        ddlClient.Items.Add(new ListItem("All clients", "0"));

        try
        {
            foreach (var client in Repositories.Users.GetClients().OrderBy(c => c.FullName))
                ddlClient.Items.Add(new ListItem(client.FullName, client.UserId.ToString()));
        }
        catch (Exception)
        {
            ShowMessage("The client list could not be loaded, so the Trip cost report will show all clients.");
        }
    }

    private void ShowOptionsForReport()
    {
        pnlClient.Visible = ddlReport.SelectedValue == ReportTypes.TripCost;

        litReportHint.Text = Server.HtmlEncode(ddlReport.SelectedValue switch
        {
            ReportTypes.Seasonal => "Trips by month, trips by province and the ten most booked activities.",
            ReportTypes.TripCost => "Cost breakdown per trip, budget versus actual, and trips that went over budget.",
            _                    => "Revenue by month, revenue by booking type and average spend per client."
        });
    }

    private void ShowMessage(string message)
    {
        litMessage.Text    = Server.HtmlEncode(message);
        pnlMessage.Visible = true;
        pnlReport.Visible  = false;
    }

    private static bool TryReadDate(string text, out DateTime date)
        => DateTime.TryParseExact(text?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out date);

    private static DataTable ToTable(ReportSection section)
    {
        var table = new DataTable();

        foreach (var column in section.Columns)
            table.Columns.Add(column.Header, typeof(string));

        foreach (var values in section.Rows)
            table.Rows.Add(Format(section, values));

        if (section.TotalRow != null && section.Rows.Count > 0)
            table.Rows.Add(Format(section, section.TotalRow));

        return table;
    }

    private static object[] Format(ReportSection section, object?[] values)
        => section.Columns
                  .Select((column, i) => (object)ReportDocument.FormatCell(i < values.Length ? values[i] : null, column.Kind))
                  .ToArray();
}
