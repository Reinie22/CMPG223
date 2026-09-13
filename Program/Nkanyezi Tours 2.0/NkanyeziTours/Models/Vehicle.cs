namespace Nkanyezi.Models;

public class Vehicle
{
    public int VehicleId { get; set; }

    public string? VehicleDescription { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Colour { get; set; }

    public string RegistrationNumber { get; set; } = "";

    public int? Capacity { get; set; }

    public string? Location { get; set; }

    public int? ProvinceId { get; set; }

    public string? ProvinceName { get; set; }

    public decimal PricePerDay { get; set; }
    public int IncludedKmPerDay { get; set; } = 200;
    public decimal RatePerExtraKm { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    public string Display
    {
        get
        {
            var name = string.Join(" ", new[] { Make, Model }
                                        .Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.IsNullOrWhiteSpace(name))
                name = string.IsNullOrWhiteSpace(VehicleDescription)
                           ? RegistrationNumber
                           : VehicleDescription!;

            return string.IsNullOrWhiteSpace(Colour) ? name : $"{name} ({Colour})";
        }
    }

    public string RateDisplay =>
        $"R{PricePerDay:N2} per day, {IncludedKmPerDay}km included per day, "
        + $"then R{RatePerExtraKm:N2} per extra km";
}
