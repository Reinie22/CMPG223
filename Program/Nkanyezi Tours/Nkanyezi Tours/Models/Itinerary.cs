namespace Nkanyezi.Models;

/// <summary>
/// One row of the Itinerary table: a trip.
///
/// The centre of the system. Every booking and every expense points at one of
/// these, and the trip summary adds them all up against TotalBudget.
///
/// AgentId is allowed to be empty. A client who builds their own trip through
/// the portal has no agent until one picks it up from the unassigned queue.
///
/// The trip dates are recorded but they do NOT restrict what can be booked - a
/// booking is free to fall outside them. That was a deliberate group decision:
/// clients extend trips constantly and blocking it caused more trouble than it
/// prevented.
/// </summary>
public class Itinerary
{
    public int ItineraryId { get; set; }

    public int ClientId { get; set; }
    public string? ClientName { get; set; }     // joined in for display
    public string? ClientEmail { get; set; }

    /// <summary>Empty until an agent claims the trip.</summary>
    public int? AgentId { get; set; }
    public string? AgentName { get; set; }      // joined in for display

    public string TripName { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal TotalBudget { get; set; }

    /// <summary>Replaces the agents' paper notebooks. Free text, can be long.</summary>
    public string? TripComments { get; set; }

    /// <summary>Draft, Confirmed, Cancelled or Completed. Use ItineraryStatus.</summary>
    public string Status { get; set; } = ItineraryStatus.Draft;

    public DateTime CreatedDate { get; set; }

    // -------------------------------------------------------------------------
    // Totals. Filled in by the summary query, left at zero elsewhere.
    // -------------------------------------------------------------------------

    /// <summary>What the agency sold: rooms, attractions, cars and seats.</summary>
    public decimal BookingsTotal { get; set; }

    /// <summary>What the client spent along the way, in Rand.</summary>
    public decimal ExpensesTotal { get; set; }

    public decimal GrandTotal => BookingsTotal + ExpensesTotal;

    /// <summary>Positive means money left, negative means the trip is over budget.</summary>
    public decimal BudgetRemaining => TotalBudget - GrandTotal;

    public bool IsOverBudget => GrandTotal > TotalBudget;

    /// <summary>How much of the budget is used, capped at 100 for the progress bar.</summary>
    public int BudgetUsedPercent =>
        TotalBudget <= 0 ? 0 : (int)Math.Min(100, Math.Round(GrandTotal / TotalBudget * 100));

    // -------------------------------------------------------------------------

    /// <summary>True for a portal trip nobody has picked up yet.</summary>
    public bool IsUnassigned => AgentId == null;

    /// <summary>
    /// A cancelled or completed trip is history. The screens use this to hide
    /// the "add booking" buttons rather than letting somebody book a car onto a
    /// trip that finished last month.
    /// </summary>
    public bool IsEditable => Status is ItineraryStatus.Draft or ItineraryStatus.Confirmed;

    public string DateRangeDisplay
    {
        get
        {
            if (StartDate == null && EndDate == null) return "Dates not set";
            if (StartDate == null) return $"until {EndDate:dd MMM yyyy}";
            if (EndDate == null)   return $"from {StartDate:dd MMM yyyy}";
            return $"{StartDate:dd MMM yyyy} - {EndDate:dd MMM yyyy}";
        }
    }

    /// <summary>Nights, when both dates are known. Zero otherwise.</summary>
    public int Nights =>
        StartDate == null || EndDate == null ? 0 : (EndDate.Value - StartDate.Value).Days;

    /// <summary>The Bootstrap badge colour for the status pill.</summary>
    public string StatusCssClass => Status switch
    {
        ItineraryStatus.Draft     => "bg-secondary",
        ItineraryStatus.Confirmed => "bg-success",
        ItineraryStatus.Completed => "bg-primary",
        ItineraryStatus.Cancelled => "bg-danger",
        _                         => "bg-secondary"
    };
}

/// <summary>
/// The four trip statuses, spelled once.
///
/// The database enforces these with a CHECK constraint, so a misspelling here
/// throws rather than saving quietly. Using the constants means it never gets
/// that far.
/// </summary>
public static class ItineraryStatus
{
    public const string Draft     = "Draft";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";

    public static readonly string[] All = { Draft, Confirmed, Cancelled, Completed };
}
