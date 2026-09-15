namespace Nkanyezi.Models;

public class TripRequestItem
{
    public int TripRequestItemId { get; set; }

    public int ItineraryId { get; set; }

    public int? TripLegId { get; set; }

    public string ItemType { get; set; } = RequestItemTypes.Accommodation;

    public int? AccommodationId { get; set; }

    public int? AttractionId { get; set; }

    public int? VehicleId { get; set; }

    public int? RouteId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Guests { get; set; } = 1;

    public string? ClientNote { get; set; }

    public string Status { get; set; } = RequestStatus.Requested;

    public string? AgentNote { get; set; }

    public DateTime CreatedDate { get; set; }

    public string ItemName { get; set; } = "";

    public string? ItemLocation { get; set; }

    public decimal GuidePrice { get; set; }

    public string? RouteType { get; set; }

    public string TypeDisplay => ItemType switch
    {
        RequestItemTypes.Vehicle => "Car rental",
        RequestItemTypes.Route   => RouteType == RouteTypes.Shuttle ? "Bus"
                                  : RouteType == RouteTypes.Flight  ? "Flight"
                                  : "Route",
        _                        => ItemType
    };

    public bool IsAccommodation => ItemType == RequestItemTypes.Accommodation;
    public bool IsAttraction    => ItemType == RequestItemTypes.Attraction;
    public bool IsVehicle       => ItemType == RequestItemTypes.Vehicle;
    public bool IsRoute         => ItemType == RequestItemTypes.Route;

    public bool IsTransport     => IsVehicle || IsRoute;

    public bool IsRequested => Status == RequestStatus.Requested;
    public bool IsApproved  => Status == RequestStatus.Approved;
    public bool IsDeclined  => Status == RequestStatus.Declined;

    public int Nights =>
        StartDate == null || EndDate == null ? 0 : (EndDate.Value - StartDate.Value).Days;

    public string DateDisplay
    {
        get
        {
            if (StartDate == null) return "No date given";

            var start = StartDate.Value.ToString("dd/MM/yyyy");

            if (EndDate == null || IsAttraction) return start;

            return $"{start} to {EndDate.Value:dd/MM/yyyy}";
        }
    }

    public decimal EstimatedCost
    {
        get
        {
            if (IsAccommodation || IsVehicle)
                return GuidePrice * Math.Max(Nights, 1);

            return GuidePrice * Guests;
        }
    }

    public string TypeCssClass => ItemType switch
    {
        RequestItemTypes.Accommodation => "text-bg-info",
        RequestItemTypes.Attraction    => "text-bg-warning text-dark",
        RequestItemTypes.Vehicle       => "text-bg-primary",
        RequestItemTypes.Route         => "text-bg-dark",
        _                              => "text-bg-light"
    };

    public string StatusCssClass => Status switch
    {
        RequestStatus.Approved => "text-bg-success",
        RequestStatus.Declined => "text-bg-danger",
        _                      => "text-bg-secondary"
    };
}

public static class RequestItemTypes
{
    public const string Accommodation = "Accommodation";
    public const string Attraction    = "Attraction";

    public const string Vehicle       = "Vehicle";

    public const string Route         = "Route";
}

public static class RequestStatus
{
    public const string Requested = "Requested";
    public const string Approved  = "Approved";
    public const string Declined  = "Declined";
}
