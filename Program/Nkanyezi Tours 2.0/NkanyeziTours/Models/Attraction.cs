namespace Nkanyezi.Models;

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

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public string Location =>
        string.Join(", ", new[] { City, ProvinceName }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

public class Lookup
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
