using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class VehicleMaintenanceRepository
{
    private readonly Db _db;

    public VehicleMaintenanceRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      vm.*,
                    v.Make,
                    v.Model,
                    v.RegistrationNumber,
                    v.Location
        FROM        dbo.VehicleMaintenance AS vm
        INNER JOIN  dbo.Vehicle AS v ON v.VehicleId = vm.VehicleId";

    public List<VehicleMaintenance> GetAll()
        => _db.Query(SelectSql + @"
        ORDER BY CASE WHEN vm.Status IN ('Scheduled', 'In progress') THEN 0 ELSE 1 END,
                 vm.StartDate, v.Location, v.Make, v.Model;",
        Map);

    public List<FleetLocationSummary> GetFleetSummary(DateTime date)
        => _db.Query(@"
        SELECT  COALESCE(NULLIF(LTRIM(RTRIM(v.Location)), ''), 'Unassigned') AS Location,
                COUNT(*) AS TotalVehicles,
                SUM(CASE WHEN activeBooking.VehicleId IS NULL
                              AND activeMaintenance.VehicleId IS NULL THEN 1 ELSE 0 END) AS AvailableVehicles,
                SUM(CASE WHEN activeBooking.VehicleId IS NOT NULL THEN 1 ELSE 0 END) AS RentedVehicles,
                SUM(CASE WHEN activeBooking.VehicleId IS NULL
                              AND activeMaintenance.VehicleId IS NOT NULL THEN 1 ELSE 0 END) AS MaintenanceVehicles
        FROM dbo.Vehicle AS v
        OUTER APPLY (
            SELECT TOP (1) vb.VehicleId
            FROM dbo.VehicleBooking AS vb
            WHERE vb.VehicleId = v.VehicleId
              AND vb.Status <> 'Cancelled'
              AND vb.StartDate <= @Date
              AND vb.EndDate >= @Date
        ) AS activeBooking
        OUTER APPLY (
            SELECT TOP (1) vm.VehicleId
            FROM dbo.VehicleMaintenance AS vm
            WHERE vm.VehicleId = v.VehicleId
              AND vm.Status IN ('Scheduled', 'In progress')
              AND vm.StartDate <= @Date
              AND vm.EndDate >= @Date
        ) AS activeMaintenance
        WHERE v.IsActive = 1
        GROUP BY COALESCE(NULLIF(LTRIM(RTRIM(v.Location)), ''), 'Unassigned')
        ORDER BY Location;",
        r => new FleetLocationSummary
        {
            Location = (string)r["Location"],
            TotalVehicles = (int)r["TotalVehicles"],
            AvailableVehicles = (int)r["AvailableVehicles"],
            RentedVehicles = (int)r["RentedVehicles"],
            MaintenanceVehicles = (int)r["MaintenanceVehicles"]
        },
        Db.P("@Date", date.Date));

    public int Create(VehicleMaintenance maintenance)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.VehicleMaintenance
              (VehicleId, MaintenanceType, StartDate, EndDate, Odometer,
               EstimatedCost, Status, Notes, CreatedBy)
        SELECT @VehicleId, @MaintenanceType, @StartDate, @EndDate, @Odometer,
               @EstimatedCost, 'Scheduled', @Notes, @CreatedBy
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.VehicleBooking AS vb
            WHERE vb.VehicleId = @VehicleId
              AND vb.Status <> 'Cancelled'
              AND vb.StartDate <= @EndDate
              AND vb.EndDate >= @StartDate)
          AND NOT EXISTS (
            SELECT 1
            FROM dbo.VehicleMaintenance AS vm WITH (UPDLOCK, HOLDLOCK)
            WHERE vm.VehicleId = @VehicleId
              AND vm.Status IN ('Scheduled', 'In progress')
              AND vm.StartDate <= @EndDate
              AND vm.EndDate >= @StartDate);

        SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS INT);",
        Db.P("@VehicleId", maintenance.VehicleId),
        Db.P("@MaintenanceType", maintenance.MaintenanceType.Trim()),
        Db.P("@StartDate", maintenance.StartDate.Date),
        Db.P("@EndDate", maintenance.EndDate.Date),
        Db.P("@Odometer", maintenance.Odometer),
        Db.P("@EstimatedCost", maintenance.EstimatedCost),
        Db.P("@Notes", string.IsNullOrWhiteSpace(maintenance.Notes) ? null : maintenance.Notes!.Trim()),
        Db.P("@CreatedBy", maintenance.CreatedBy));

    public bool Complete(int maintenanceId, decimal? actualCost)
        => _db.Execute(@"
        UPDATE dbo.VehicleMaintenance
        SET Status = 'Completed',
            CompletedDate = SYSDATETIME(),
            ActualCost = @ActualCost
        WHERE VehicleMaintenanceId = @MaintenanceId
          AND Status IN ('Scheduled', 'In progress');",
        Db.P("@MaintenanceId", maintenanceId),
        Db.P("@ActualCost", actualCost)) > 0;

    private static VehicleMaintenance Map(SqlDataReader r)
    {
        var vehicleName = string.Join(" ", new[]
        {
            Db.GetNullableString(r, "Make"),
            Db.GetNullableString(r, "Model")
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return new VehicleMaintenance
        {
            VehicleMaintenanceId = (int)r["VehicleMaintenanceId"],
            VehicleId = (int)r["VehicleId"],
            VehicleDisplay = vehicleName,
            RegistrationNumber = (string)r["RegistrationNumber"],
            Location = Db.GetOptionalString(r, "Location") ?? "",
            MaintenanceType = (string)r["MaintenanceType"],
            StartDate = (DateTime)r["StartDate"],
            EndDate = (DateTime)r["EndDate"],
            Odometer = Db.GetNullable<int>(r, "Odometer"),
            EstimatedCost = Db.GetNullable<decimal>(r, "EstimatedCost"),
            ActualCost = Db.GetNullable<decimal>(r, "ActualCost"),
            Status = (string)r["Status"],
            Notes = Db.GetNullableString(r, "Notes"),
            CompletedDate = Db.GetNullable<DateTime>(r, "CompletedDate"),
            CreatedDate = (DateTime)r["CreatedDate"],
            CreatedBy = Db.GetNullable<int>(r, "CreatedBy")
        };
    }
}
