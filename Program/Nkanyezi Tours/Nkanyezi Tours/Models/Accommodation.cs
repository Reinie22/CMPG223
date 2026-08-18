namespace Nkanyezi.Models;

/// <summary>
/// One row of the Accommodation table: a hotel, guest house or lodge.
///
/// A model class holds data and nothing else: no SQL, no rules, no database
/// calls. Property names match the column names exactly. Keep it that way -
/// it makes the repository obvious and nobody has to look up what maps to what.
/// </summary>
public class Accommodation
{
    public int AccommodationId { get; set; }

    public string Name { get; set; } = "";
    public string? Description { get; set; }

    /// <summary>Hotel, guest house, lodge. Free text in the database.</summary>
    public string? Type { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public int? ProvinceId { get; set; }

    /// <summary>Joined in from the Province table for display. Not a column here.</summary>
    public string? ProvinceName { get; set; }

    public byte? StarRating { get; set; }
    public decimal PricePerNight { get; set; }

    /// <summary>Total people the property can sleep.</summary>
    public int Capacity { get; set; }

    public string? Amenities { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    /// <summary>Counted by the list query so the screen can show "12 rooms".</summary>
    public int RoomCount { get; set; }

    public string Location =>
        string.Join(", ", new[] { City, ProvinceName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    /// <summary>"4 stars", or a dash when the rating was never captured.</summary>
    public string StarDisplay =>
        StarRating == null ? "-" : new string('*', StarRating.Value) + $" ({StarRating})";
}

/// <summary>
/// One room inside one accommodation.
///
/// Rooms are booked individually: a client taking three rooms produces three
/// RoomBooking rows, each naming the exact room. That is why the room number
/// has to be unique within its property.
/// </summary>
public class Room
{
    public int RoomId { get; set; }
    public int AccommodationId { get; set; }

    /// <summary>Joined in for display on the booking screens.</summary>
    public string? AccommodationName { get; set; }

    public string RoomNumber { get; set; } = "";
    public string? RoomType { get; set; }
    public int Beds { get; set; }
    public string? Size { get; set; }

    /// <summary>True when the room has its own bathroom.</summary>
    public bool Bathroom { get; set; } = true;

    /// <summary>
    /// Whether the room is in service at all. This is NOT the same as "free on
    /// these dates" - a room can be available in general and still be booked
    /// next week. The date check is done by the booking query.
    /// </summary>
    public bool AvailabilityStatus { get; set; } = true;

    /// <summary>
    /// Copied down from the parent accommodation by the availability query, so
    /// the booking screen can price a room without a second trip to the
    /// database.
    /// </summary>
    public decimal PricePerNight { get; set; }

    public string Display => $"Room {RoomNumber}"
                             + (string.IsNullOrWhiteSpace(RoomType) ? "" : $" ({RoomType})")
                             + $" - {Beds} bed" + (Beds == 1 ? "" : "s");
}
