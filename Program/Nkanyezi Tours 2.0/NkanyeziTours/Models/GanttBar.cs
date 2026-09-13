namespace Nkanyezi.Models;

public class GanttBar
{
    public string GroupName { get; set; } = "";

    public int RowId { get; set; }

    public string RowName { get; set; } = "";

    public string? RowDetail { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Reference { get; set; }

    public string? Status { get; set; }

    public string? TripName { get; set; }

    public bool IsDraft { get; set; }

    public bool HasBar => StartDate != null && EndDate != null;

    public bool Covers(DateTime day)
        => HasBar && day.Date >= StartDate!.Value.Date && day.Date < EndDate!.Value.Date;

    public DateTime? LastDay => HasBar ? EndDate!.Value.Date.AddDays(-1) : (DateTime?)null;

    public string Tooltip =>
        (IsDraft ? "Requested: " : "Booked: ")
        + Reference
        + (string.IsNullOrWhiteSpace(TripName) ? "" : " - " + TripName)
        + (HasBar
              ? $" ({StartDate:dd/MM/yyyy} to {LastDay:dd/MM/yyyy}, "
                + $"free again {EndDate:dd/MM/yyyy})"
              : "");
}
