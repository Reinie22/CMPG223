namespace Nkanyezi.Models;

public static class MaintenanceStatus
{
    public const string Scheduled = "Scheduled";
    public const string InProgress = "In progress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public class VehicleMaintenance
{
    public int VehicleMaintenanceId { get; set; }
    public int VehicleId { get; set; }
    public string VehicleDisplay { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public string Location { get; set; } = "";
    public string MaintenanceType { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? Odometer { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string Status { get; set; } = MaintenanceStatus.Scheduled;
    public string? Notes { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }

    public string DateRangeDisplay => StartDate == EndDate
        ? StartDate.ToString("dd MMM yyyy")
        : $"{StartDate:dd MMM yyyy} – {EndDate:dd MMM yyyy}";

    public bool CanComplete =>
        Status == MaintenanceStatus.Scheduled || Status == MaintenanceStatus.InProgress;

    public bool IsOverdue => CanComplete && EndDate.Date < DateTime.Today;
}

public class FleetLocationSummary
{
    public string Location { get; set; } = "Unassigned";
    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int RentedVehicles { get; set; }
    public int MaintenanceVehicles { get; set; }

    public string StockLevel => AvailableVehicles <= 1
        ? "Shortage"
        : AvailableVehicles >= 3 ? "Surplus" : "Balanced";

    public string StockCss => StockLevel == "Shortage"
        ? "danger"
        : StockLevel == "Surplus" ? "success" : "warning";
}
