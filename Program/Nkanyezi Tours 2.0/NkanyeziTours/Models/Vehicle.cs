namespace Nkanyezi.Models;

/// <summary>
/// One row of the Vehicle table: a car available for hire.
///
/// Pricing works as the group agreed: a set price per day which includes
/// IncludedKmPerDay kilometres for each day of the rental, and anything driven
/// above that is charged at RatePerExtraKm. The extra kilometres are only known
/// once the car comes back and the odometer is read, which is why a vehicle
/// booking is the only booking whose final cost is not known when it is made.
/// </summary>
public class Vehicle
{
    public int VehicleId { get; set; }

    public string? VehicleDescription { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Colour { get; set; }

    /// <summary>Unique across the whole table. Two cars cannot share a plate.</summary>
    public string RegistrationNumber { get; set; } = "";

    /// <summary>Passengers, including the driver.</summary>
    public int? Capacity { get; set; }

    /// <summary>The branch where the client collects the car.</summary>
    public string? Location { get; set; }

    public decimal PricePerDay { get; set; }
    public int IncludedKmPerDay { get; set; } = 200;
    public decimal RatePerExtraKm { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }

    /// <summary>"Toyota Corolla (White)" - what the dropdowns show.</summary>
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

    /// <summary>The pricing sentence the booking screen shows under the dropdown.</summary>
    public string RateDisplay =>
        $"R{PricePerDay:N2} per day, {IncludedKmPerDay}km included per day, "
        + $"then R{RatePerExtraKm:N2} per extra km";
}
