namespace Nkanyezi.Models;

/// <summary>
/// A room booked on a trip for a range of dates.
///
/// One row per room. A client taking three rooms produces three rows, each
/// naming the exact room, because a single row claiming "3 rooms" cannot tell
/// you which rooms are actually occupied - and without that the clash check is
/// impossible.
/// </summary>
public class RoomBooking
{
    public int RoomBookingId { get; set; }
    public int RoomId { get; set; }
    public int ItineraryId { get; set; }

    // Joined in for display, so the itinerary page can show a booking without
    // fetching the room and the property separately.
    public string? RoomNumber { get; set; }
    public string? RoomType { get; set; }
    public int? AccommodationId { get; set; }
    public string? AccommodationName { get; set; }
    public string? City { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    /// <summary>
    /// Worked out when the booking is made, from nights multiplied by the
    /// nightly price, and then stored. It is stored rather than recalculated so
    /// that raising a hotel's price next month does not silently change what a
    /// past client was charged.
    /// </summary>
    public decimal TotalCost { get; set; }

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    /// <summary>Nights, which is what a hotel actually charges for.</summary>
    public int Nights => (CheckOutDate - CheckInDate).Days;

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public string Display =>
        $"{AccommodationName} - room {RoomNumber}, {Nights} night" + (Nights == 1 ? "" : "s");
}

/// <summary>
/// A visit to an attraction, on one date, in one time slot.
///
/// The time slot is required. Capacity is counted per slot, so a booking with
/// no slot could not be capacity checked and the whole availability feature
/// would fall over.
/// </summary>
public class AttractionBooking
{
    public int AttractionBookingId { get; set; }
    public int AttractionId { get; set; }
    public int ItineraryId { get; set; }
    public int TimeSlotId { get; set; }

    // Joined in for display
    public string? AttractionName { get; set; }
    public string? City { get; set; }
    public TimeSpan? TimeStart { get; set; }
    public TimeSpan? TimeEnd { get; set; }

    public DateTime BookingDate { get; set; }

    /// <summary>Number of people.</summary>
    public int Quantity { get; set; }

    public decimal TotalCost { get; set; }

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public string SlotDisplay =>
        TimeStart == null ? "" : $"{TimeStart:hh\\:mm} - {TimeEnd:hh\\:mm}";

    public string Display =>
        $"{AttractionName} on {BookingDate:dd MMM yyyy}, {Quantity} "
        + (Quantity == 1 ? "person" : "people");
}

/// <summary>
/// A car hire on a trip.
///
/// The only booking whose final cost is not known when it is made, because it
/// depends on how far the client drives.
///
/// Everything from TotalDays down to TotalCost is a COMPUTED column: SQL Server
/// works it out from the other columns in the row. That means these values are
/// read-only here. Never put them in an INSERT or UPDATE - SQL Server rejects
/// writes to a computed column, and the error message is not helpful.
///
/// The three rate columns are copied from the Vehicle when the booking is made
/// rather than looked up later. If the booking only pointed at the vehicle,
/// raising that car's daily price next month would silently change what every
/// past client was charged and the revenue report would stop matching the
/// invoices.
/// </summary>
public class VehicleBooking
{
    public int VehicleBookingId { get; set; }
    public int VehicleId { get; set; }
    public int ItineraryId { get; set; }

    // Joined in for display
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Location { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Rates frozen at the moment of hire
    public decimal RatePerDay { get; set; }
    public int IncludedKmPerDay { get; set; }
    public decimal RatePerExtraKm { get; set; }

    /// <summary>Captured when the client collects the car. Empty until then.</summary>
    public int? OdometerOut { get; set; }

    /// <summary>Captured when the car comes back. Empty until then.</summary>
    public int? OdometerIn { get; set; }

    // ---- computed by SQL Server, read-only ----
    public int TotalDays { get; set; }
    public int? KmDriven { get; set; }
    public int KmIncluded { get; set; }
    public int ExtraKm { get; set; }
    public decimal BaseCost { get; set; }
    public decimal ExtraKmCost { get; set; }
    public decimal TotalCost { get; set; }
    // ------------------------------------------

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    /// <summary>True once the car is out and the opening odometer is captured.</summary>
    public bool IsCollected => Status == BookingStatus.Collected;

    /// <summary>True once the car is back and the cost is final.</summary>
    public bool IsReturned => Status == BookingStatus.Returned;

    /// <summary>
    /// Until the car comes back, TotalCost is only the daily charge. The screens
    /// use this to label it "estimated" rather than presenting a figure that is
    /// about to change.
    /// </summary>
    public bool IsCostFinal => IsReturned;

    public string VehicleDisplay =>
        string.Join(" ", new[] { Make, Model }.Where(x => !string.IsNullOrWhiteSpace(x)))
        + (string.IsNullOrWhiteSpace(RegistrationNumber) ? "" : $" ({RegistrationNumber})");

    public string Display =>
        $"{VehicleDisplay}, {TotalDays} day" + (TotalDays == 1 ? "" : "s");
}

/// <summary>
/// A seat booked on a flight or a bus shuttle.
///
/// Seats multiplied by the fare is known the moment the booking is made.
/// Availability is tracked per route and travel date against the route's seat
/// capacity, for both flights and bus shuttles.
///
/// TotalCost is a COMPUTED column (Seats * FarePerSeat). Read-only - never
/// insert or update it.
/// </summary>
public class RouteBooking
{
    public int RouteBookingId { get; set; }
    public int RouteId { get; set; }
    public int ItineraryId { get; set; }

    // Joined in for display
    public string? RouteType { get; set; }
    public string? Carrier { get; set; }
    public string? RouteCode { get; set; }
    public string? DepartureCity { get; set; }
    public string? ArrivalCity { get; set; }
    public TimeSpan? DepartureTime { get; set; }

    public DateTime TravelDate { get; set; }
    public int Seats { get; set; }

    /// <summary>
    /// Pre-filled from the route's indicative fare and then corrected by the
    /// agent to what was actually paid, so the trip total reflects real money
    /// rather than the guide price.
    /// </summary>
    public decimal FarePerSeat { get; set; }

    /// <summary>Computed by SQL Server as Seats * FarePerSeat. Read-only.</summary>
    public decimal TotalCost { get; set; }

    /// <summary>The Nkanyezi reference generated when the booking is confirmed.</summary>
    public string? BookingReference { get; set; }

    public string Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    public string Journey => $"{DepartureCity} to {ArrivalCity}";

    public string Display =>
        $"{Carrier} {RouteCode} - {Journey} on {TravelDate:dd MMM yyyy}, "
        + $"{Seats} seat" + (Seats == 1 ? "" : "s");
}

/// <summary>
/// One line on the trip summary, whatever kind of booking it came from.
///
/// The itinerary detail page shows rooms, attractions, cars and seats in a
/// single table sorted by date. Rather than four loops in the view, the
/// repository turns all four into this shape.
/// </summary>
public class BookingLine
{
    /// <summary>"Accommodation", "Attraction", "Vehicle" or "Route".</summary>
    public string BookingType { get; set; } = "";

    /// <summary>The id within that type, for the edit and cancel links.</summary>
    public int BookingId { get; set; }

    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = BookingStatus.Confirmed;

    public bool IsCancelled => Status == BookingStatus.Cancelled;

    /// <summary>The Bootstrap icon or badge colour, so the table reads at a glance.</summary>
    public string TypeCssClass => BookingType switch
    {
        "Accommodation" => "bg-info",
        "Attraction"    => "bg-warning text-dark",
        "Vehicle"       => "bg-primary",
        "Route"         => "bg-dark",
        _               => "bg-secondary"
    };
}

/// <summary>
/// The booking statuses, spelled once.
///
/// Rooms, attractions and routes only ever use Confirmed and Cancelled. A
/// vehicle booking also passes through Collected and Returned, because its cost
/// is not settled until the odometer comes back. The database enforces the
/// vehicle list with a CHECK constraint.
/// </summary>
public static class BookingStatus
{
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";

    /// <summary>Vehicle bookings only: the car is out.</summary>
    public const string Collected = "Collected";

    /// <summary>Vehicle bookings only: the car is back and the cost is final.</summary>
    public const string Returned  = "Returned";

    public static readonly string[] Standard = { Confirmed, Cancelled };
    public static readonly string[] Vehicle  = { Confirmed, Collected, Returned, Cancelled };
}
