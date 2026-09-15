namespace Nkanyezi.Models;

public class RoomBooking
{
    public int RoomBookingId { get; set; }
    public int RoomId { get; set; }
    public int ItineraryId { get; set; }

    public string? RoomNumber { get; set; }
    public string? RoomType { get; set; }
    public int? AccommodationId { get; set; }
    public string? AccommodationName { get; set; }
    public string? City { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    public decimal TotalCost { get; set; }

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public int Nights => (CheckOutDate - CheckInDate).Days;

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public string Display =>
        $"{AccommodationName} - room {RoomNumber}, {Nights} night" + (Nights == 1 ? "" : "s");
}

public class AttractionBooking
{
    public int AttractionBookingId { get; set; }
    public int AttractionId { get; set; }
    public int ItineraryId { get; set; }

    public string? AttractionName { get; set; }
    public string? City { get; set; }

    public DateTime BookingDate { get; set; }

    public int Quantity { get; set; }

    public decimal TotalCost { get; set; }

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public string Display =>
        $"{AttractionName} on {BookingDate:dd/MM/yyyy}, {Quantity} "
        + (Quantity == 1 ? "person" : "people");
}

public class VehicleBooking
{
    public int VehicleBookingId { get; set; }
    public int VehicleId { get; set; }
    public int ItineraryId { get; set; }

    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Location { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal RatePerDay { get; set; }
    public int IncludedKmPerDay { get; set; }
    public decimal RatePerExtraKm { get; set; }

    public int? OdometerOut { get; set; }

    public int? OdometerIn { get; set; }

    public int TotalDays { get; set; }
    public int? KmDriven { get; set; }
    public int KmIncluded { get; set; }
    public int ExtraKm { get; set; }
    public decimal BaseCost { get; set; }
    public decimal ExtraKmCost { get; set; }
    public decimal TotalCost { get; set; }

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public bool IsCollected => Status == BookingStatus.Collected;

    public bool IsReturned => Status == BookingStatus.Returned;

    public bool IsCostFinal => IsReturned;

    public string VehicleDisplay =>
        string.Join(" ", new[] { Make, Model }.Where(x => !string.IsNullOrWhiteSpace(x)))
        + (string.IsNullOrWhiteSpace(RegistrationNumber) ? "" : $" ({RegistrationNumber})");

    public string Display =>
        $"{VehicleDisplay}, {TotalDays} day" + (TotalDays == 1 ? "" : "s");
}

public class RouteBooking
{
    public int RouteBookingId { get; set; }
    public int RouteId { get; set; }
    public int ItineraryId { get; set; }

    public string? RouteType { get; set; }
    public string? Carrier { get; set; }
    public string? RouteCode { get; set; }
    public string? DepartureCity { get; set; }
    public string? ArrivalCity { get; set; }
    public TimeSpan? DepartureTime { get; set; }

    public DateTime TravelDate { get; set; }
    public int Seats { get; set; }

    public decimal FarePerSeat { get; set; }

    public decimal TotalCost { get; set; }

    public string? BookingReference { get; set; }

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public string Journey => $"{DepartureCity} to {ArrivalCity}";

    public string Display =>
        $"{Carrier} {RouteCode} - {Journey} on {TravelDate:dd/MM/yyyy}, "
        + $"{Seats} seat" + (Seats == 1 ? "" : "s");
}

public class BookingLine
{
    public string BookingType { get; set; } = "";

    public int BookingId { get; set; }

    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = BookingStatus.Confirmed;

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public string TypeCssClass => BookingType switch
    {
        "Accommodation" => "bg-info",
        "Attraction"    => "bg-warning text-dark",
        "Car rental"    => "bg-primary",
        "Flight"        => "bg-dark",
        "Bus"           => "bg-dark",
        _               => "bg-secondary"
    };

    public string TypeDisplay => BookingType == "Attraction" ? "Activity" : BookingType;
}

public static class BookingStatus
{
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";

    public const string Collected = "Collected";

    public const string Returned  = "Returned";

    public static readonly string[] Standard = { Confirmed, Cancelled };
    public static readonly string[] Vehicle  = { Confirmed, Collected, Returned, Cancelled };
}
