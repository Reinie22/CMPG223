using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class GanttRepository
{
    private readonly Db _db;

    public GanttRepository(Db db)
    {
        _db = db;
    }

    public List<GanttBar> GetResortBars(DateTime monthStart)
        => _db.Query(@"
        DECLARE @From DATE = @MonthStart;
        DECLARE @To   DATE = DATEADD(month, 1, @MonthStart);

        SELECT      a.Name                          AS GroupName,
                    r.RoomId                        AS RowId,
                    r.RoomNumber                    AS RowName,
                    r.RoomType                      AS RowDetail,
                    bars.StartDate,
                    bars.EndDate,
                    bars.Reference,
                    bars.Status,
                    bars.IsDraft,
                    bars.TripName
        FROM        dbo.Room          AS r
        INNER JOIN  dbo.Accommodation AS a ON a.AccommodationId = r.AccommodationId
        LEFT JOIN   (

            SELECT      rb.RoomId,
                        rb.CheckInDate  AS StartDate,
                        rb.CheckOutDate AS EndDate,
                        CONCAT('RB-', rb.RoomBookingId) AS Reference,
                        rb.Status,
                        CAST(0 AS BIT)  AS IsDraft,
                        i.TripName
            FROM        dbo.RoomBooking AS rb
            INNER JOIN  dbo.Itinerary   AS i ON i.ItineraryId = rb.ItineraryId
            WHERE       rb.Status <> 'Cancelled'
              AND       rb.CheckInDate  < @To
              AND       rb.CheckOutDate > @From

            UNION ALL

            SELECT      lr.RoomId,
                        l.StartDate,
                        l.EndDate,
                        CONCAT('LEG-', l.TripLegId) AS Reference,
                        CASE l.ResortStatus WHEN 'Approved' THEN 'Booked' ELSE 'Requested' END AS Status,
                        CAST(CASE l.ResortStatus WHEN 'Approved' THEN 0 ELSE 1 END AS BIT) AS IsDraft,
                        i.TripName
            FROM        dbo.TripLegRoom AS lr
            INNER JOIN  dbo.TripLeg     AS l ON l.TripLegId   = lr.TripLegId
            INNER JOIN  dbo.Itinerary   AS i ON i.ItineraryId = l.ItineraryId
            WHERE       i.Status NOT IN ('Cancelled','Completed')
              AND       l.ResortStatus <> 'Declined'
              AND       l.StartDate < @To
              AND       l.EndDate   > @From

            UNION ALL

            SELECT      (SELECT MIN(r2.RoomId) FROM dbo.Room AS r2
                         WHERE r2.AccommodationId = l.AccommodationId) AS RoomId,
                        l.StartDate,
                        l.EndDate,
                        CONCAT('LEG-', l.TripLegId) AS Reference,
                        CASE l.ResortStatus WHEN 'Approved' THEN 'Booked' ELSE 'Requested' END AS Status,
                        CAST(CASE l.ResortStatus WHEN 'Approved' THEN 0 ELSE 1 END AS BIT) AS IsDraft,
                        i.TripName
            FROM        dbo.TripLeg   AS l
            INNER JOIN  dbo.Itinerary AS i ON i.ItineraryId = l.ItineraryId
            WHERE       l.AccommodationId IS NOT NULL
              AND       l.ResortStatus <> 'Declined'
              AND       NOT EXISTS (SELECT 1 FROM dbo.TripLegRoom AS x
                                    WHERE  x.TripLegId = l.TripLegId)
              AND       i.Status NOT IN ('Cancelled','Completed')
              AND       l.StartDate < @To
              AND       l.EndDate   > @From
        ) AS bars ON bars.RoomId = r.RoomId
        WHERE       a.IsActive = 1
        ORDER BY    a.Name, r.RoomNumber, bars.StartDate;",
        Map,
        Db.P("@MonthStart", monthStart.Date));

    public List<GanttBar> GetVehicleBars(DateTime monthStart)
        => _db.Query(@"
        DECLARE @From DATE = @MonthStart;
        DECLARE @To   DATE = DATEADD(month, 1, @MonthStart);

        SELECT      ISNULL(p.ProvinceName, 'No region set') AS GroupName,
                    v.VehicleId                             AS RowId,
                    v.RegistrationNumber                    AS RowName,
                    CONCAT(v.Make, ' ', v.Model)            AS RowDetail,
                    bars.StartDate,
                    bars.EndDate,
                    bars.Reference,
                    bars.Status,
                    bars.IsDraft,
                    bars.TripName
        FROM        dbo.Vehicle  AS v
        LEFT JOIN   dbo.Province AS p ON p.ProvinceId = v.ProvinceId
        LEFT JOIN   (

            SELECT      vb.VehicleId,
                        vb.StartDate,
                        DATEADD(day, 1, vb.EndDate) AS EndDate,
                        CONCAT('VB-', vb.VehicleBookingId) AS Reference,
                        vb.Status,
                        CAST(0 AS BIT) AS IsDraft,
                        i.TripName
            FROM        dbo.VehicleBooking AS vb
            INNER JOIN  dbo.Itinerary      AS i ON i.ItineraryId = vb.ItineraryId
            WHERE       vb.Status <> 'Cancelled'
              AND       vb.StartDate                <  @To
              AND       DATEADD(day, 1, vb.EndDate) >  @From

            UNION ALL

            SELECT      tr.VehicleId,
                        l.StartDate,
                        l.EndDate,
                        CONCAT('LEG-', l.TripLegId) AS Reference,
                        'Requested'    AS Status,
                        CAST(1 AS BIT) AS IsDraft,
                        i.TripName
            FROM        dbo.TripRequestItem AS tr
            INNER JOIN  dbo.TripLeg         AS l ON l.TripLegId   = tr.TripLegId
            INNER JOIN  dbo.Itinerary       AS i ON i.ItineraryId = l.ItineraryId
            WHERE       tr.ItemType = 'Vehicle'
              AND       tr.Status  <> 'Declined'
              AND       i.Status NOT IN ('Cancelled','Completed')
              AND       l.StartDate < @To
              AND       l.EndDate   > @From

            UNION ALL

            SELECT      vm.VehicleId,
                        vm.StartDate,
                        DATEADD(day, 1, vm.EndDate) AS EndDate,
                        CONCAT('WS-', vm.VehicleMaintenanceId) AS Reference,
                        vm.Status,
                        CAST(0 AS BIT) AS IsDraft,
                        CONCAT('In for ', vm.MaintenanceType) AS TripName
            FROM        dbo.VehicleMaintenance AS vm
            WHERE       vm.Status IN ('Scheduled', 'In progress')
              AND       vm.StartDate                <  @To
              AND       DATEADD(day, 1, vm.EndDate) >  @From
        ) AS bars ON bars.VehicleId = v.VehicleId
        WHERE       v.IsActive = 1
        ORDER BY    GroupName, v.RegistrationNumber, bars.StartDate;",
        Map,
        Db.P("@MonthStart", monthStart.Date));

    private static GanttBar Map(SqlDataReader r) => new GanttBar
    {
        GroupName = (string)r["GroupName"],
        RowId     = (int)r["RowId"],
        RowName   = (string)r["RowName"],
        RowDetail = Db.GetNullableString(r, "RowDetail"),

        StartDate = Db.GetNullable<DateTime>(r, "StartDate"),
        EndDate   = Db.GetNullable<DateTime>(r, "EndDate"),
        Reference = Db.GetNullableString(r, "Reference"),
        Status    = Db.GetNullableString(r, "Status"),
        TripName  = Db.GetNullableString(r, "TripName"),
        IsDraft   = Db.GetNullable<bool>(r, "IsDraft") ?? false
    };
}
