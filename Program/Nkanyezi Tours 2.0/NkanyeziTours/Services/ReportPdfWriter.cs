using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using Nkanyezi.Models;

namespace Nkanyezi.Services;

public static class ReportPdfWriter
{
    private const string FontName = "Arial";

    private static readonly Color Navy      = Color.FromRgb(0x1F, 0x33, 0x51);
    private static readonly Color Muted     = Color.FromRgb(0x6B, 0x7C, 0x93);
    private static readonly Color Stripe    = Color.FromRgb(0xF4, 0xF6, 0xF9);
    private static readonly Color TotalFill = Color.FromRgb(0xE9, 0xEE, 0xF5);
    private static readonly Color Line      = Color.FromRgb(0xCF, 0xD8, 0xE3);

    private static readonly object FontLock = new object();
    private static bool _fontsReady;

    public static byte[] Write(ReportDocument report)
    {
        EnsureFonts();

        var document = new Document();
        document.Info.Title  = "Nkanyezi Tours - " + report.Title;
        document.Info.Author = "Nkanyezi Tours";

        DefineStyles(document);

        var section = document.AddSection();
        section.PageSetup = document.DefaultPageSetup.Clone();
        section.PageSetup.PageFormat   = PageFormat.A4;
        section.PageSetup.Orientation  = Orientation.Portrait;
        section.PageSetup.PageWidth    = Unit.FromCentimeter(report.Landscape ? 29.7 : 21.0);
        section.PageSetup.PageHeight   = Unit.FromCentimeter(report.Landscape ? 21.0 : 29.7);
        section.PageSetup.TopMargin    = Unit.FromCentimeter(2.2);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(2.0);
        section.PageSetup.LeftMargin   = Unit.FromCentimeter(1.6);
        section.PageSetup.RightMargin  = Unit.FromCentimeter(1.6);

        var usableWidth = (report.Landscape ? 29.7 : 21.0) - 3.2;

        AddHeaderAndFooter(section, report, usableWidth);

        section.AddParagraph(report.Title, "Title");
        section.AddParagraph(report.Description, "Lead");

        AddDetails(section, report, usableWidth);
        AddStats(section, report, usableWidth);

        foreach (var reportSection in report.Sections)
            AddSection(section, reportSection, usableWidth);

        AddLegend(section, report, usableWidth);

        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private static void EnsureFonts()
    {
        if (_fontsReady)
            return;

        lock (FontLock)
        {
            if (_fontsReady)
                return;

            if (GlobalFontSettings.FontResolver == null)
                GlobalFontSettings.UseWindowsFontsUnderWindows = true;

            _fontsReady = true;
        }
    }

    private static void DefineStyles(Document document)
    {
        var normal = document.Styles[StyleNames.Normal]!;
        normal.Font.Name = FontName;
        normal.Font.Size = 9;

        var title = document.Styles.AddStyle("Title", StyleNames.Normal);
        title.Font.Size = 18;
        title.Font.Bold = true;
        title.Font.Color = Navy;
        title.ParagraphFormat.SpaceAfter = Unit.FromPoint(2);

        var lead = document.Styles.AddStyle("Lead", StyleNames.Normal);
        lead.Font.Size = 10;
        lead.Font.Color = Muted;
        lead.ParagraphFormat.SpaceAfter = Unit.FromPoint(10);

        var heading = document.Styles.AddStyle("SectionHeading", StyleNames.Normal);
        heading.Font.Size = 13;
        heading.Font.Bold = true;
        heading.Font.Color = Navy;
        heading.ParagraphFormat.SpaceBefore = Unit.FromPoint(14);
        heading.ParagraphFormat.SpaceAfter = Unit.FromPoint(2);
        heading.ParagraphFormat.KeepWithNext = true;

        var note = document.Styles.AddStyle("SectionNote", StyleNames.Normal);
        note.Font.Italic = true;
        note.Font.Color = Muted;
        note.ParagraphFormat.SpaceAfter = Unit.FromPoint(5);
        note.ParagraphFormat.KeepWithNext = true;

        var small = document.Styles.AddStyle("Small", StyleNames.Normal);
        small.Font.Size = 8;
        small.Font.Color = Muted;
    }

    private static void AddHeaderAndFooter(Section section, ReportDocument report, double width)
    {
        var header = section.Headers.Primary.AddParagraph();
        header.Style = "Small";
        header.AddFormattedText("Nkanyezi Tours", TextFormat.Bold);
        header.AddText("  |  " + report.Title + "  |  " + report.PeriodDisplay);
        header.Format.Borders.Bottom.Width = 0.5;
        header.Format.Borders.Bottom.Color = Line;

        var footer = section.Footers.Primary.AddTable();
        footer.Borders.Visible = false;
        footer.AddColumn(Unit.FromCentimeter(width * 0.7));
        footer.AddColumn(Unit.FromCentimeter(width * 0.3));

        var row = footer.AddRow();
        var left = row.Cells[0].AddParagraph(report.GeneratedDisplay);
        left.Style = "Small";

        var right = row.Cells[1].AddParagraph();
        right.Style = "Small";
        right.Format.Alignment = ParagraphAlignment.Right;
        right.AddText("Page ");
        right.AddPageField();
        right.AddText(" of ");
        right.AddNumPagesField();
    }

    private static void AddDetails(Section section, ReportDocument report, double usableWidth)
    {
        var table = section.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromCentimeter(3.2));
        table.AddColumn(Unit.FromCentimeter(usableWidth - 3.2));

        foreach (var filter in report.Filters)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(filter.Key).Format.Font.Bold = true;
            row.Cells[1].AddParagraph(filter.Value);
        }

        var generated = table.AddRow();
        generated.Cells[0].AddParagraph("Generated").Format.Font.Bold = true;
        generated.Cells[1].AddParagraph(report.GeneratedDisplay.Replace("Generated on ", ""));

        section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(6);
    }

    private static void AddStats(Section section, ReportDocument report, double usableWidth)
    {
        if (report.Stats.Count == 0)
            return;

        var table = section.AddTable();
        table.Borders.Color = Line;
        table.Borders.Width = 0.5;
        table.LeftPadding = Unit.FromPoint(6);
        table.TopPadding = Unit.FromPoint(4);
        table.BottomPadding = Unit.FromPoint(4);

        var columnWidth = usableWidth / report.Stats.Count;
        foreach (var _ in report.Stats)
            table.AddColumn(Unit.FromCentimeter(columnWidth));

        var labels = table.AddRow();
        labels.Shading.Color = Stripe;
        var values = table.AddRow();

        for (var i = 0; i < report.Stats.Count; i++)
        {
            var label = labels.Cells[i].AddParagraph(report.Stats[i].Label);
            label.Format.Font.Color = Muted;
            label.Format.Font.Size = 8;

            var value = values.Cells[i].AddParagraph(report.Stats[i].Value);
            value.Format.Font.Size = 12;
            value.Format.Font.Bold = true;
        }
    }

    private static void AddSection(Section section, ReportSection reportSection, double usableWidth)
    {
        section.AddParagraph(reportSection.Heading, "SectionHeading");
        section.AddParagraph(reportSection.Description, "SectionNote");

        if (reportSection.Rows.Count == 0)
        {
            section.AddParagraph(reportSection.EmptyText).Format.Font.Italic = true;
            return;
        }

        var table = section.AddTable();
        table.Borders.Color = Line;
        table.Borders.Width = 0.5;
        table.LeftPadding = Unit.FromPoint(4);
        table.RightPadding = Unit.FromPoint(4);
        table.TopPadding = Unit.FromPoint(2.5);
        table.BottomPadding = Unit.FromPoint(2.5);

        var totalWeight = reportSection.Columns.Sum(c => c.PdfWidth);
        foreach (var column in reportSection.Columns)
        {
            var added = table.AddColumn(Unit.FromCentimeter(usableWidth * column.PdfWidth / totalWeight));
            added.Format.Alignment = column.IsNumeric ? ParagraphAlignment.Right : ParagraphAlignment.Left;
        }

        var header = table.AddRow();
        header.HeadingFormat = true;
        header.Shading.Color = Navy;
        header.Format.Font.Bold = true;
        header.Format.Font.Color = Colors.White;
        for (var c = 0; c < reportSection.Columns.Count; c++)
            header.Cells[c].AddParagraph(reportSection.Columns[c].Header);

        var index = 0;
        foreach (var values in reportSection.Rows)
        {
            var row = AddDataRow(table, reportSection, values);
            if (index++ % 2 == 1)
                row.Shading.Color = Stripe;
        }

        if (reportSection.TotalRow != null)
        {
            var total = AddDataRow(table, reportSection, reportSection.TotalRow);
            total.Format.Font.Bold = true;
            total.Shading.Color = TotalFill;
        }
    }

    private static Row AddDataRow(Table table, ReportSection reportSection, object?[] values)
    {
        var row = table.AddRow();
        for (var c = 0; c < reportSection.Columns.Count && c < values.Length; c++)
            row.Cells[c].AddParagraph(ReportDocument.FormatCell(values[c], reportSection.Columns[c].Kind));
        return row;
    }

    private static void AddLegend(Section section, ReportDocument report, double usableWidth)
    {
        if (report.Legend.Count == 0)
            return;

        section.AddParagraph("Legend", "SectionHeading");
        section.AddParagraph("What the terms and short forms in this report mean.", "SectionNote");

        var table = section.AddTable();
        table.Borders.Color = Line;
        table.Borders.Width = 0.5;
        table.LeftPadding = Unit.FromPoint(4);
        table.TopPadding = Unit.FromPoint(2.5);
        table.BottomPadding = Unit.FromPoint(2.5);
        table.AddColumn(Unit.FromCentimeter(3.6));
        table.AddColumn(Unit.FromCentimeter(usableWidth - 3.6));

        foreach (var item in report.Legend)
        {
            var row = table.AddRow();
            row.KeepWith = 0;
            row.Cells[0].AddParagraph(item.Term).Format.Font.Bold = true;
            row.Cells[1].AddParagraph(item.Meaning);
        }
    }
}
