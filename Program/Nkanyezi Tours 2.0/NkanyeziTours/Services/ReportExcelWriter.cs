using System.IO;
using ClosedXML.Excel;
using Nkanyezi.Models;

namespace Nkanyezi.Services;

public static class ReportExcelWriter
{
    private const string MoneyFormat   = "\"R\"#,##0.00";
    private const string NumberFormat  = "#,##0";
    private const string PercentFormat = "0.0%";
    private const string DateFormat    = "dd/mm/yyyy";

    private static readonly XLColor HeaderFill = XLColor.FromHtml("#1F3351");
    private static readonly XLColor TotalFill  = XLColor.FromHtml("#E9EEF5");

    public static byte[] Write(ReportDocument document)
    {
        using var workbook = new XLWorkbook();

        var sheetNames = document.Sections
                                 .Select((s, i) => SheetName(s.Heading, i + 2))
                                 .ToList();

        WriteSummary(workbook.Worksheets.Add("Summary"), document, sheetNames);

        for (var i = 0; i < document.Sections.Count; i++)
            WriteSection(workbook.Worksheets.Add(sheetNames[i]), document, document.Sections[i]);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteSummary(IXLWorksheet sheet, ReportDocument document, List<string> sheetNames)
    {
        var row = WriteTitle(sheet, document, document.Title, document.Description);

        row = WriteHeading(sheet, row, "Report details");
        foreach (var filter in document.Filters)
            row = WritePair(sheet, row, filter.Key, filter.Value);

        row = WriteHeading(sheet, row + 1, "Key figures");
        foreach (var stat in document.Stats)
            row = WritePair(sheet, row, stat.Label, stat.Value);

        row = WriteHeading(sheet, row + 1, "Contents");
        for (var i = 0; i < document.Sections.Count; i++)
            row = WritePair(sheet, row, document.Sections[i].Heading, "See the \"" + sheetNames[i] + "\" sheet");

        row = WriteHeading(sheet, row + 1, "Legend");
        foreach (var item in document.Legend)
            row = WritePair(sheet, row, item.Term, item.Meaning);

        sheet.Column(1).Width = 28;
        sheet.Column(2).Width = 90;
        sheet.Column(2).Style.Alignment.WrapText = true;
        sheet.Column(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

        SetUpPage(sheet, document, 1);
    }

    private static void WriteSection(IXLWorksheet sheet, ReportDocument document, ReportSection section)
    {
        var row = WriteTitle(sheet, document, document.Title + " - " + section.Heading, section.Description);

        var headerRow = row;
        for (var c = 0; c < section.Columns.Count; c++)
        {
            var cell = sheet.Cell(headerRow, c + 1);
            cell.Value = section.Columns[c].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = HeaderFill;
            cell.Style.Alignment.Horizontal = section.Columns[c].IsNumeric
                ? XLAlignmentHorizontalValues.Right
                : XLAlignmentHorizontalValues.Left;
        }

        row++;

        if (section.Rows.Count == 0)
        {
            sheet.Cell(row, 1).Value = section.EmptyText;
            sheet.Cell(row, 1).Style.Font.Italic = true;
            row++;
        }

        foreach (var values in section.Rows)
            WriteRow(sheet, row++, section, values, false);

        if (section.TotalRow != null)
            WriteRow(sheet, row++, section, section.TotalRow, true);

        var lastColumn = Math.Max(section.Columns.Count, 1);
        sheet.Range(headerRow, 1, Math.Max(row - 1, headerRow), lastColumn)
             .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        sheet.Columns(1, lastColumn).AdjustToContents(headerRow, Math.Max(row - 1, headerRow));
        for (var c = 1; c <= lastColumn; c++)
            sheet.Column(c).Width = Math.Max(sheet.Column(c).Width + 2, 12);

        sheet.Cell(row + 1, 1).Value = "The legend for this report is on the Summary sheet.";
        sheet.Cell(row + 1, 1).Style.Font.Italic = true;
        sheet.Cell(row + 1, 1).Style.Font.FontColor = XLColor.Gray;

        sheet.SheetView.FreezeRows(headerRow);
        SetUpPage(sheet, document, headerRow);
    }

    private static void WriteRow(IXLWorksheet sheet, int row, ReportSection section, object?[] values, bool isTotal)
    {
        for (var c = 0; c < section.Columns.Count && c < values.Length; c++)
        {
            var cell   = sheet.Cell(row, c + 1);
            var column = section.Columns[c];
            var value  = values[c];

            if (value == null)
            {
                cell.Value = "";
            }
            else
            {
                switch (column.Kind)
                {
                    case ReportColumnKind.Money:
                        cell.Value = Convert.ToDecimal(value);
                        cell.Style.NumberFormat.Format = MoneyFormat;
                        break;
                    case ReportColumnKind.Number:
                        cell.Value = Convert.ToDecimal(value);
                        cell.Style.NumberFormat.Format = NumberFormat;
                        break;
                    case ReportColumnKind.Percent:
                        cell.Value = Convert.ToDecimal(value) / 100m;
                        cell.Style.NumberFormat.Format = PercentFormat;
                        break;
                    case ReportColumnKind.Date:
                        if (value is DateTime date)
                        {
                            cell.Value = date;
                            cell.Style.NumberFormat.Format = DateFormat;
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        }
                        else
                        {
                            cell.Value = value.ToString();
                        }
                        break;
                    default:
                        cell.Value = value.ToString();
                        break;
                }
            }

            if (isTotal)
            {
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = TotalFill;
                cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            }
        }
    }

    private static int WriteTitle(IXLWorksheet sheet, ReportDocument document, string title, string description)
    {
        sheet.Cell(1, 1).Value = title;
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 16;

        sheet.Cell(2, 1).Value = "Nkanyezi Tours  |  Period: " + document.PeriodDisplay;
        sheet.Cell(3, 1).Value = document.GeneratedDisplay;
        sheet.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;

        sheet.Cell(4, 1).Value = description;
        sheet.Cell(4, 1).Style.Font.Italic = true;

        return 6;
    }

    private static int WriteHeading(IXLWorksheet sheet, int row, string text)
    {
        var cell = sheet.Cell(row, 1);
        cell.Value = text;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 13;
        cell.Style.Font.FontColor = HeaderFill;
        return row + 1;
    }

    private static int WritePair(IXLWorksheet sheet, int row, string label, string value)
    {
        sheet.Cell(row, 1).Value = label;
        sheet.Cell(row, 1).Style.Font.Bold = true;
        sheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        sheet.Cell(row, 2).Value = value;
        return row + 1;
    }

    private static void SetUpPage(IXLWorksheet sheet, ReportDocument document, int repeatUpToRow)
    {
        sheet.PageSetup.PageOrientation = document.Landscape
            ? XLPageOrientation.Landscape
            : XLPageOrientation.Portrait;
        sheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
        sheet.PageSetup.FitToPages(1, 0);
        sheet.PageSetup.SetRowsToRepeatAtTop(1, repeatUpToRow);
        sheet.PageSetup.Header.Left.AddText("Nkanyezi Tours - " + document.Title);
        sheet.PageSetup.Footer.Left.AddText(document.GeneratedDisplay);
        sheet.PageSetup.Footer.Right.AddText("Page &P of &N");
    }

    private static string SheetName(string heading, int number)
    {
        var name = new string(heading.Where(ch => "[]*?/\\:".IndexOf(ch) < 0).ToArray()).Trim();

        if (name.Length > 31)
            name = name.Substring(0, 31).Trim();

        return name.Length == 0 ? "Sheet " + number : name;
    }
}
