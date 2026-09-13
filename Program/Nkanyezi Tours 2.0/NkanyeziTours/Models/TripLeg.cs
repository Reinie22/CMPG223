namespace Nkanyezi.Models;

public class TripLeg
{
    public int TripLegId { get; set; }

    public int ItineraryId { get; set; }

    public int LegNumber { get; set; }

    public int? ProvinceId { get; set; }

    public string? ProvinceName { get; set; }

    public string? Destination { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int? AccommodationId { get; set; }

    public int Guests { get; set; } = 2;

    public string? ClientNote { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ResortName { get; set; }

    public string? ResortLocation { get; set; }

    public string? ResortType { get; set; }

    public decimal ResortPricePerNight { get; set; }

    public string? ResortImageUrl { get; set; }

    public int ActivityCount { get; set; }

    public decimal ActivityCost { get; set; }

    public int TransportCount { get; set; }

    public decimal TransportCost { get; set; }

    public bool HasResort => AccommodationId != null;

    public int RoomCount { get; set; }

    public int RoomsCapacity { get; set; }

    public string? RoomNames { get; set; }

    public bool HasUnit => RoomCount > 0;

    public string UnitDisplay =>
        RoomCount == 0
            ? "No rooms chosen yet"
            : (RoomCount == 1 ? "Room " : "Rooms ") + RoomNames;

    public bool UnitTooSmall => RoomCount > 0 && Guests > RoomsCapacity;

    public int Nights => (EndDate - StartDate).Days;

    public string RegionDisplay
    {
        get
        {
            var parts = new[] { Destination, ProvinceName }
                        .Where(x => !string.IsNullOrWhiteSpace(x));

            var text = string.Join(", ", parts);

            return text.Length == 0 ? "Not said yet" : text;
        }
    }

    public string Title
    {
        get
        {
            var where = string.IsNullOrWhiteSpace(Destination) ? ProvinceName : Destination;

            return string.IsNullOrWhiteSpace(where)
                ? $"Leg {LegNumber}"
                : $"Leg {LegNumber} - {where}";
        }
    }

    public string DateDisplay =>
        $"{StartDate:dd/MM/yyyy} to {EndDate:dd/MM/yyyy}, "
        + Nights + (Nights == 1 ? " night" : " nights");

    public decimal EstimatedResortCost => ResortPricePerNight * Nights;

    public decimal LegExpenses => EstimatedResortCost + ActivityCost + TransportCost;
}
