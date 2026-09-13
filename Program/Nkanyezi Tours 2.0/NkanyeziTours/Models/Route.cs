namespace Nkanyezi.Models;

public class Route
{
    public int RouteId { get; set; }

    public string RouteType { get; set; } = RouteTypes.Flight;

    public string Carrier { get; set; } = "";

    public string? RouteCode { get; set; }

    public string DepartureCity { get; set; } = "";
    public string ArrivalCity { get; set; } = "";

    public TimeSpan? DepartureTime { get; set; }
    public TimeSpan? ArrivalTime { get; set; }
    public int? DurationMinutes { get; set; }

    public decimal IndicativeFarePerSeat { get; set; }

    public int SeatCapacity { get; set; } = 1;

    public int? SeatsAvailable { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public bool IsFlight => RouteType == RouteTypes.Flight;

    public string TypeDisplay => RouteType == RouteTypes.Shuttle ? "Bus" : RouteType;

    public string Journey => $"{DepartureCity} to {ArrivalCity}";

    public string Display =>
        string.IsNullOrWhiteSpace(RouteCode) ? $"{Carrier} - {Journey}"
                                             : $"{Carrier} {RouteCode} - {Journey}";

    public string BookingDisplay =>
        $"{Display} — {SeatsAvailable ?? SeatCapacity} of {SeatCapacity} seats available";

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

public static class RouteTypes
{
    public const string Flight  = "Flight";
    public const string Shuttle = "Shuttle";

    public static readonly string[] All = { Flight, Shuttle };
}
