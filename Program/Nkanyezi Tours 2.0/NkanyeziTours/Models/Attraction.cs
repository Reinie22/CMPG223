namespace Nkanyezi.Models;

/// <summary>
/// One row of the Attraction table.
///
/// This is a "model" class. It holds data and nothing else: no SQL, no rules,
/// no calculations. Every part of the system passes these around, which is why
/// they live in their own project that everything else can reference.
///
/// The property names match the column names exactly. Keep it that way. It
/// makes the repository code obvious and it means nobody has to look up what
/// maps to what.
/// </summary>
public class Attraction
{

    public int AttractionId { get; set; }


    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }

    public decimal Rate { get; set; }

    public int Quantity { get; set; }

    public string? UnitOfMeasure { get; set; }


    public int? DurationMinutes { get; set; }
    public TimeSpan? OperatingTimeStart { get; set; }
    public TimeSpan? OperatingTimeEnd { get; set; }
    public string? BestOperatingSeason { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public string Location =>
        string.Join(", ", new[] { City, ProvinceName }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

public class TimeSlot
{
    public int TimeSlotId { get; set; }
    public int AttractionId { get; set; }
    public TimeSpan TimeStart { get; set; }
    public TimeSpan TimeEnd { get; set; }

    public string Display => $"{TimeStart:hh\\:mm} - {TimeEnd:hh\\:mm}";
}

public class TimeSlotAvailability
{
    public int TimeSlotId { get; set; }
    public TimeSpan TimeStart { get; set; }
    public TimeSpan TimeEnd { get; set; }
    public int Capacity { get; set; }
    public int AlreadyBooked { get; set; }
    public int SpacesLeft { get; set; }

    public bool IsFull => SpacesLeft <= 0;
    public string Display => $"{TimeStart:hh\\:mm} - {TimeEnd:hh\\:mm}  ({SpacesLeft} of {Capacity} left)";
}

public class Lookup
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
