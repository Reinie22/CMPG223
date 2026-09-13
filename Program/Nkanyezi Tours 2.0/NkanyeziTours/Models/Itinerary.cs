namespace Nkanyezi.Models;

public class Itinerary
{
    public int ItineraryId { get; set; }

    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }

    public int? AgentId { get; set; }
    public string? AgentName { get; set; }

    public string TripName { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal TotalBudget { get; set; }

    public string? TripComments { get; set; }

    public string Status { get; set; } = ItineraryStatus.Draft;

    public DateTime CreatedDate { get; set; }

    public decimal BookingsTotal { get; set; }

    public decimal ExpensesTotal { get; set; }

    public decimal GrandTotal => BookingsTotal + ExpensesTotal;

    public decimal BudgetRemaining => TotalBudget - GrandTotal;

    public bool IsOverBudget => GrandTotal > TotalBudget;

    public int BudgetUsedPercent =>
        TotalBudget <= 0 ? 0 : (int)Math.Min(100, Math.Round(GrandTotal / TotalBudget * 100));

    public bool IsUnassigned => AgentId == null;

    public bool IsEditable =>
        Status is ItineraryStatus.Draft or ItineraryStatus.Confirmed
        && (EndDate == null || EndDate.Value.Date >= DateTime.Today);

    public bool ClientCanEdit => IsEditable && Status == ItineraryStatus.Draft;

    public bool HasPassed =>
        EndDate != null && EndDate.Value.Date < DateTime.Today
        && Status is ItineraryStatus.Draft or ItineraryStatus.Confirmed;

    public string DateRangeDisplay
    {
        get
        {
            if (StartDate == null && EndDate == null) return "Dates not set";
            if (StartDate == null) return $"until {EndDate:dd/MM/yyyy}";
            if (EndDate == null)   return $"from {StartDate:dd/MM/yyyy}";
            return $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}";
        }
    }

    public int Nights =>
        StartDate == null || EndDate == null ? 0 : (EndDate.Value - StartDate.Value).Days;

    public string StatusCssClass => Status switch
    {
        ItineraryStatus.Draft     => "bg-secondary",
        ItineraryStatus.Confirmed => "bg-success",
        ItineraryStatus.Completed => "bg-primary",
        ItineraryStatus.Cancelled => "bg-danger",
        _                         => "bg-secondary"
    };
}

public static class ItineraryStatus
{
    public const string Draft     = "Draft";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";

    public static readonly string[] All = { Draft, Confirmed, Cancelled, Completed };
}
