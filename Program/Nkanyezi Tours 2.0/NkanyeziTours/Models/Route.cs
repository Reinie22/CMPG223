namespace Nkanyezi.Models;

/// <summary>
/// One row of the Route table: a flight or a bus shuttle.
///
/// Flights and shuttles share one table because both are the same thing - a
/// journey from one city to another at a set time, priced per seat. The only
/// real difference is who operates it, so RouteType is a column rather than a
/// second set of screens.
///
/// IndicativeFarePerSeat is a guide price captured by the Administrator so a
/// client browsing the portal can see roughly what a trip costs. It is not a
/// quote. When the agent actually books, the real fare is captured on the
/// booking and it is that figure the client is charged.
/// </summary>
public class Route
{
    public int RouteId { get; set; }

    /// <summary>"Flight" or "Shuttle". Use the RouteTypes constants.</summary>
    public string RouteType { get; set; } = RouteTypes.Flight;

    /// <summary>The airline or the bus company.</summary>
    public string Carrier { get; set; } = "";

    /// <summary>Flight number or service number.</summary>
    public string? RouteCode { get; set; }

    public string DepartureCity { get; set; } = "";
    public string ArrivalCity { get; set; } = "";

    public TimeSpan? DepartureTime { get; set; }
    public TimeSpan? ArrivalTime { get; set; }
    public int? DurationMinutes { get; set; }

    public decimal IndicativeFarePerSeat { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public bool IsFlight => RouteType == RouteTypes.Flight;

    /// <summary>"Johannesburg to Cape Town".</summary>
    public string Journey => $"{DepartureCity} to {ArrivalCity}";

    /// <summary>"SA 315 - Johannesburg to Cape Town" for the dropdowns.</summary>
    public string Display =>
        string.IsNullOrWhiteSpace(RouteCode) ? $"{Carrier} - {Journey}"
                                             : $"{Carrier} {RouteCode} - {Journey}";

    /// <summary>"2h 15m", or empty when the duration was never captured.</summary>
    public string DurationDisplay
    {
        get
        {
            if (DurationMinutes == null) return "";
            var hours = DurationMinutes.Value / 60;
            var minutes = DurationMinutes.Value % 60;
            return hours == 0 ? $"{minutes}m" : $"{hours}h {minutes:00}m";
        }
    }
}

/// <summary>
/// The two route types, spelled once.
///
/// The database has CHECK (RouteType IN ('Flight','Shuttle')) on this column,
/// so a typo here does not silently save bad data - it throws. Using the
/// constants means it never gets that far.
/// </summary>
public static class RouteTypes
{
    public const string Flight  = "Flight";
    public const string Shuttle = "Shuttle";

    public static readonly string[] All = { Flight, Shuttle };
}
