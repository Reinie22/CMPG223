namespace Nkanyezi.Models;

public static class ReportTypes
{
    public const string Revenue  = "Revenue";
    public const string Seasonal = "Seasonal";
    public const string TripCost = "TripCost";
}

public enum ReportColumnKind
{
    Text,
    Money,
    Number,
    Percent,
    Date,
    Month
}

public class ReportColumn
{
    public ReportColumn(string header, ReportColumnKind kind = ReportColumnKind.Text, double pdfWidth = 3)
    {
        Header   = header;
        Kind     = kind;
        PdfWidth = pdfWidth;
    }

    public string Header { get; }

    public ReportColumnKind Kind { get; }

    public double PdfWidth { get; }

    public bool IsNumeric => Kind is ReportColumnKind.Money or ReportColumnKind.Number or ReportColumnKind.Percent;
}

public class ReportSection
{
    public string Key { get; set; } = "";

    public string Heading { get; set; } = "";

    public string Description { get; set; } = "";

    public string EmptyText { get; set; } = "Nothing to show for the dates chosen.";

    public List<ReportColumn> Columns { get; } = new List<ReportColumn>();

    public List<object?[]> Rows { get; } = new List<object?[]>();

    public object?[]? TotalRow { get; set; }

    public void SortBy(int columnIndex, bool descending)
    {
        if (columnIndex < 0 || columnIndex >= Columns.Count || Rows.Count < 2)
            return;

        var comparer = Comparer<object?>.Create(CompareCells);

        var sorted = descending
            ? Rows.OrderByDescending(r => columnIndex < r.Length ? r[columnIndex] : null, comparer).ToList()
            : Rows.OrderBy(r => columnIndex < r.Length ? r[columnIndex] : null, comparer).ToList();

        Rows.Clear();
        Rows.AddRange(sorted);

        var direction = Columns[columnIndex].Kind switch
        {
            ReportColumnKind.Text                          => descending ? "Z to A" : "A to Z",
            ReportColumnKind.Date or ReportColumnKind.Month => descending ? "latest first" : "earliest first",
            _                                              => descending ? "highest first" : "lowest first"
        };

        Description = (Description.TrimEnd() + " Sorted by " + Columns[columnIndex].Header.ToLowerInvariant()
                      + ", " + direction + ".").Trim();
    }

    private static int CompareCells(object? a, object? b)
    {
        if (a == null || a == DBNull.Value)
            return b == null || b == DBNull.Value ? 0 : -1;

        if (b == null || b == DBNull.Value)
            return 1;

        if (a is DateTime da && b is DateTime db)
            return da.CompareTo(db);

        if (IsNumber(a) && IsNumber(b))
            return Convert.ToDecimal(a).CompareTo(Convert.ToDecimal(b));

        return string.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool IsNumber(object value)
        => value is decimal or int or long or double or float or short or byte;
}

public class ReportStat
{
    public ReportStat(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }

    public string Value { get; }
}

public class ReportLegendItem
{
    public ReportLegendItem(string term, string meaning)
    {
        Term    = term;
        Meaning = meaning;
    }

    public string Term { get; }

    public string Meaning { get; }
}

public class ReportDocument
{
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public bool HasClientFilter { get; set; }

    public string? SectionShown { get; set; }

    public string? ClientName { get; set; }

    public DateTime GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = "";

    public bool Landscape { get; set; }

    public List<ReportStat> Stats { get; } = new List<ReportStat>();

    public List<ReportSection> Sections { get; } = new List<ReportSection>();

    public List<ReportLegendItem> Legend { get; } = new List<ReportLegendItem>();

    public string PeriodDisplay => $"{FromDate:dd/MM/yyyy} to {ToDate:dd/MM/yyyy}";

    public string GeneratedDisplay =>
        $"Generated on {GeneratedAt:dd/MM/yyyy} at {GeneratedAt:HH:mm}"
        + (string.IsNullOrWhiteSpace(GeneratedBy) ? "" : " by " + GeneratedBy);

    public List<KeyValuePair<string, string>> Filters
    {
        get
        {
            var filters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Period", PeriodDisplay),
                new KeyValuePair<string, string>("Trips included", "Confirmed and completed trips that start in this period")
            };

            if (HasClientFilter)
                filters.Add(new KeyValuePair<string, string>("Client",
                    string.IsNullOrWhiteSpace(ClientName) ? "All clients" : ClientName!));

            if (!string.IsNullOrWhiteSpace(SectionShown))
                filters.Add(new KeyValuePair<string, string>("Part shown", SectionShown!));

            return filters;
        }
    }

    public string FileName(string extension)
    {
        var name = $"Nkanyezi Tours - {Title} - {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}.{extension}";

        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            name = name.Replace(c, '-');

        return name;
    }

    public static string FormatCell(object? value, ReportColumnKind kind)
    {
        if (value == null || value == DBNull.Value)
            return "";

        switch (kind)
        {
            case ReportColumnKind.Money:
                var amount = Convert.ToDecimal(value);
                return (amount < 0 ? "-R" : "R") + Math.Abs(amount).ToString("N2");
            case ReportColumnKind.Number:
                return Convert.ToDecimal(value).ToString("N0");
            case ReportColumnKind.Percent:
                return Convert.ToDecimal(value).ToString("0.0") + "%";
            case ReportColumnKind.Date:
                return value is DateTime date ? date.ToString("dd/MM/yyyy") : value.ToString();
            case ReportColumnKind.Month:
                return value is DateTime month ? month.ToString("MMMM yyyy") : value.ToString();
            default:
                return value.ToString() ?? "";
        }
    }
}

public class ReportTripValue
{
    public int ItineraryId { get; set; }

    public string TripName { get; set; } = "";

    public int ClientId { get; set; }

    public string ClientName { get; set; } = "";

    public string? AgentName { get; set; }

    public DateTime TripDate { get; set; }

    public string Status { get; set; } = "";

    public decimal TotalBudget { get; set; }

    public string Source { get; set; } = "";

    public decimal Accommodation { get; set; }

    public decimal Activities { get; set; }

    public decimal CarRental { get; set; }

    public decimal FlightsAndBuses { get; set; }

    public decimal Expenses { get; set; }

    public decimal Transport => CarRental + FlightsAndBuses;

    public decimal Revenue => Accommodation + Activities + CarRental + FlightsAndBuses;

    public decimal TotalCost => Revenue + Expenses;

    public decimal BudgetDifference => TotalBudget - TotalCost;

    public bool OverBudget => TotalCost > TotalBudget;
}

public class ReportProvinceCount
{
    public string ProvinceName { get; set; } = "";

    public int Trips { get; set; }
}

public class ReportActivityCount
{
    public string ActivityName { get; set; } = "";

    public string? ActivityType { get; set; }

    public string? ProvinceName { get; set; }

    public int Trips { get; set; }

    public int People { get; set; }
}
