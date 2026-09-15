using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class VehicleRepository
{
    private readonly Db _db;

    public VehicleRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      v.*,
                    p.ProvinceName
        FROM        dbo.Vehicle  AS v
        LEFT JOIN   dbo.Province AS p ON p.ProvinceId = v.ProvinceId";

    public List<Vehicle> GetAll(bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR v.IsActive = 1)
        ORDER BY v.Make, v.Model;",
        Map,
        Db.P("@IncludeInactive", includeInactive));

    public Vehicle? GetById(int vehicleId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   v.VehicleId = @VehicleId;",
        Map,
        Db.P("@VehicleId", vehicleId));

    public List<Vehicle> Search(string? text = null, string? location = null,
                                int? minCapacity = null, decimal? maxPricePerDay = null,
                                bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR v.IsActive = 1)
          AND   (@Text IS NULL
                 OR v.Make               LIKE @Text ESCAPE '\'
                 OR v.Model              LIKE @Text ESCAPE '\'
                 OR v.RegistrationNumber LIKE @Text ESCAPE '\'
                 OR v.VehicleDescription LIKE @Text ESCAPE '\')
          AND   (@Location    IS NULL OR v.Location     = @Location)
          AND   (@MinCapacity IS NULL OR v.Capacity    >= @MinCapacity)
          AND   (@MaxPrice    IS NULL OR v.PricePerDay <= @MaxPrice)
        ORDER BY v.Make, v.Model;",
        Map,
        Db.Like("@Text", text),
        Db.P("@Location",        string.IsNullOrWhiteSpace(location) ? null : location),
        Db.P("@MinCapacity",     minCapacity),
        Db.P("@MaxPrice",        maxPricePerDay),
        Db.P("@IncludeInactive", includeInactive));

    public List<Vehicle> GetAvailable(DateTime startDate, DateTime endDate)
        => _db.Query(SelectSql + @"
        WHERE   v.IsActive = 1
          AND   NOT EXISTS (
                    SELECT  1
                    FROM    dbo.VehicleBooking AS vb
                    WHERE   vb.VehicleId = v.VehicleId
                      AND   vb.Status   <> 'Cancelled'
                      AND   vb.StartDate <= @EndDate
                      AND   vb.EndDate   >= @StartDate)

          AND   NOT EXISTS (
                    SELECT  1
                    FROM    dbo.VehicleMaintenance AS vm
                    WHERE   vm.VehicleId = v.VehicleId
                      AND   vm.Status IN ('Scheduled', 'In progress')
                      AND   vm.StartDate <= @EndDate
                      AND   vm.EndDate   >= @StartDate)
        ORDER BY v.Make, v.Model;",
        Map,
        Db.P("@StartDate", startDate.Date),
        Db.P("@EndDate",   endDate.Date));

    public bool IsAvailable(int vehicleId, DateTime startDate, DateTime endDate,
                            int? excludeVehicleBookingId = null)
        => _db.Scalar<int>(@"

        SELECT  COUNT(*)
        FROM (
            SELECT  VehicleBookingId AS ConflictId
            FROM    dbo.VehicleBooking
            WHERE   VehicleId = @VehicleId
              AND   Status   <> 'Cancelled'
              AND   StartDate <= @EndDate
              AND   EndDate   >= @StartDate
              AND   (@ExcludeId IS NULL OR VehicleBookingId <> @ExcludeId)

            UNION ALL

            SELECT  VehicleMaintenanceId
            FROM    dbo.VehicleMaintenance
            WHERE   VehicleId = @VehicleId
              AND   Status IN ('Scheduled', 'In progress')
              AND   StartDate <= @EndDate
              AND   EndDate   >= @StartDate
        ) AS conflicts;",
        Db.P("@VehicleId", vehicleId),
        Db.P("@StartDate", startDate.Date),
        Db.P("@EndDate",   endDate.Date),
        Db.P("@ExcludeId", excludeVehicleBookingId)) == 0;

    public bool RegistrationExists(string registrationNumber, int? excludeVehicleId = null)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.Vehicle
        WHERE   RegistrationNumber = @Registration
          AND   (@ExcludeId IS NULL OR VehicleId <> @ExcludeId);",
        Db.P("@Registration", registrationNumber.Trim()),
        Db.P("@ExcludeId",    excludeVehicleId)) > 0;

    public int Create(Vehicle v)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Vehicle
              (VehicleDescription,  Make,  Model,  Colour,  RegistrationNumber,
               Capacity,  Location,  ProvinceId,  PricePerDay,  IncludedKmPerDay,  RatePerExtraKm)
        VALUES
              (@VehicleDescription, @Make, @Model, @Colour, @RegistrationNumber,
               @Capacity, @Location, @ProvinceId, @PricePerDay, @IncludedKmPerDay, @RatePerExtraKm);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        BuildParameters(v));

    public void Update(Vehicle v)
    {
        var parameters = new List<SqlParameter> { Db.P("@VehicleId", v.VehicleId) };
        parameters.AddRange(BuildParameters(v));

        _db.Execute(@"
        UPDATE  dbo.Vehicle
        SET     VehicleDescription = @VehicleDescription,
                Make               = @Make,
                Model              = @Model,
                Colour             = @Colour,
                RegistrationNumber = @RegistrationNumber,
                Capacity           = @Capacity,
                Location           = @Location,
                ProvinceId         = @ProvinceId,
                PricePerDay        = @PricePerDay,
                IncludedKmPerDay   = @IncludedKmPerDay,
                RatePerExtraKm     = @RatePerExtraKm
        WHERE   VehicleId = @VehicleId;",
        parameters.ToArray());
    }

    public bool Delete(int vehicleId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1 FROM dbo.VehicleBooking     WHERE VehicleId = @VehicleId)
                   OR EXISTS (SELECT 1 FROM dbo.TripRequestItem    WHERE VehicleId = @VehicleId)
                   OR EXISTS (SELECT 1 FROM dbo.VehicleMaintenance WHERE VehicleId = @VehicleId)
                 THEN 1 ELSE 0 END;

        IF @HasBookings = 1
            UPDATE dbo.Vehicle SET IsActive = 0 WHERE VehicleId = @VehicleId;
        ELSE
            DELETE FROM dbo.Vehicle WHERE VehicleId = @VehicleId;

        SELECT @HasBookings;",
        Db.P("@VehicleId", vehicleId));

    public void Reactivate(int vehicleId)
        => _db.Execute(@"
        UPDATE dbo.Vehicle SET IsActive = 1 WHERE VehicleId = @VehicleId;",
        Db.P("@VehicleId", vehicleId));

    private static Vehicle Map(SqlDataReader r) => new Vehicle
    {
        VehicleId          = (int)r["VehicleId"],
        VehicleDescription = Db.GetNullableString(r, "VehicleDescription"),
        Make               = Db.GetNullableString(r, "Make"),
        Model              = Db.GetNullableString(r, "Model"),
        Colour             = Db.GetNullableString(r, "Colour"),
        RegistrationNumber = (string)r["RegistrationNumber"],
        Capacity           = Db.GetNullable<int>(r, "Capacity"),
        Location           = Db.GetNullableString(r, "Location"),
        ProvinceId         = Db.GetNullable<int>(r, "ProvinceId"),
        ProvinceName       = Db.GetOptionalString(r, "ProvinceName"),
        PricePerDay        = (decimal)r["PricePerDay"],
        IncludedKmPerDay   = (int)r["IncludedKmPerDay"],
        RatePerExtraKm     = (decimal)r["RatePerExtraKm"],
        IsActive           = (bool)r["IsActive"],
        CreatedDate        = (DateTime)r["CreatedDate"]
    };

    private static SqlParameter[] BuildParameters(Vehicle v) => new[]
    {
        Db.P("@VehicleDescription", v.VehicleDescription),
        Db.P("@Make",               v.Make),
        Db.P("@Model",              v.Model),
        Db.P("@Colour",             v.Colour),
        Db.P("@RegistrationNumber", v.RegistrationNumber.Trim()),
        Db.P("@Capacity",           v.Capacity),
        Db.P("@Location",           v.Location),
        Db.P("@ProvinceId",         v.ProvinceId),
        Db.P("@PricePerDay",        v.PricePerDay),
        Db.P("@IncludedKmPerDay",   v.IncludedKmPerDay),
        Db.P("@RatePerExtraKm",     v.RatePerExtraKm)
    };
}
