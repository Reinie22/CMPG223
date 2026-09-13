namespace Nkanyezi.Models;

public class Accommodation
{
    public int AccommodationId { get; set; }

    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public string? Type { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public int? ProvinceId { get; set; }

    public string? ProvinceName { get; set; }

    public byte? StarRating { get; set; }
    public decimal PricePerNight { get; set; }

    public int Capacity { get; set; }

    public string? Amenities { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public int RoomCount { get; set; }

    public decimal FromPricePerNight { get; set; }

    public int TotalCapacity { get; set; }

    public string? ImageUrl { get; set; }

    public string Location =>
        string.Join(", ", new[] { City, ProvinceName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public string StarDisplay =>
        StarRating == null ? "-" : new string('*', StarRating.Value) + $" ({StarRating})";
}

public class Room
{
    public int RoomId { get; set; }
    public int AccommodationId { get; set; }

    public string? AccommodationName { get; set; }

    public string RoomNumber { get; set; } = "";
    public string? RoomType { get; set; }
    public int Beds { get; set; }
    public string? Size { get; set; }

    public bool Bathroom { get; set; } = true;

    public bool AvailabilityStatus { get; set; } = true;

    public decimal PricePerNight { get; set; }

    public int Capacity { get; set; }

    public string Display => $"Room {RoomNumber}"
                             + (string.IsNullOrWhiteSpace(RoomType) ? "" : $" ({RoomType})")
                             + $" - {Beds} bed" + (Beds == 1 ? "" : "s");
}
