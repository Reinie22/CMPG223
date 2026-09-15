using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class RouteRepository
{
    private readonly Db _db;

    public RouteRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT  r.*
        FROM    dbo.Route AS r";

    public List<Route> GetAll(bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR r.IsActive = 1)
        ORDER BY r.DepartureCity, r.ArrivalCity, r.DepartureTime;",
        Map,
        Db.P("@IncludeInactive", includeInactive));

    public Route? GetById(int routeId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   r.RouteId = @RouteId;",
        Map,
        Db.P("@RouteId", routeId));

    public List<Route> Search(string? routeType = null, string? departureCity = null,
                              string? arrivalCity = null, string? carrier = null,
                              decimal? maxFare = null, bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR r.IsActive = 1)
          AND   (@RouteType     IS NULL OR r.RouteType     = @RouteType)
          AND   (@DepartureCity IS NULL OR r.DepartureCity = @DepartureCity)
          AND   (@ArrivalCity   IS NULL OR r.ArrivalCity   = @ArrivalCity)
          AND   (@Carrier       IS NULL OR r.Carrier   LIKE @Carrier ESCAPE '\')
          AND   (@MaxFare       IS NULL OR r.IndicativeFarePerSeat <= @MaxFare)
        ORDER BY r.DepartureCity, r.ArrivalCity, r.DepartureTime;",
        Map,
        Db.P("@RouteType",     string.IsNullOrWhiteSpace(routeType)     ? null : routeType),
        Db.P("@DepartureCity", string.IsNullOrWhiteSpace(departureCity) ? null : departureCity),
        Db.P("@ArrivalCity",   string.IsNullOrWhiteSpace(arrivalCity)   ? null : arrivalCity),
        Db.Like("@Carrier", carrier),
        Db.P("@MaxFare",         maxFare),
        Db.P("@IncludeInactive", includeInactive));

    public List<Route> GetByType(string routeType) => Search(routeType: routeType);

    public List<Route> GetAvailabilityByType(string routeType, DateTime travelDate)
        => _db.Query(@"
        SELECT  r.*, availability.SeatsAvailable
        FROM    dbo.Route AS r
        CROSS APPLY (
            SELECT r.SeatCapacity - ISNULL(SUM(rb.Seats), 0) AS SeatsAvailable
            FROM   dbo.RouteBooking AS rb
            WHERE  rb.RouteId = r.RouteId
              AND  rb.TravelDate = @TravelDate
              AND  rb.Status <> 'Cancelled'
        ) AS availability
        WHERE   r.IsActive = 1
          AND   r.RouteType = @RouteType
        ORDER BY r.DepartureCity, r.ArrivalCity, r.DepartureTime;",
        Map,
        Db.P("@TravelDate", travelDate.Date),
        Db.P("@RouteType", routeType));

    public Route? GetAvailability(int routeId, DateTime travelDate)
        => _db.QuerySingle(@"
        SELECT  r.*, availability.SeatsAvailable
        FROM    dbo.Route AS r
        CROSS APPLY (
            SELECT r.SeatCapacity - ISNULL(SUM(rb.Seats), 0) AS SeatsAvailable
            FROM   dbo.RouteBooking AS rb
            WHERE  rb.RouteId = r.RouteId
              AND  rb.TravelDate = @TravelDate
              AND  rb.Status <> 'Cancelled'
        ) AS availability
        WHERE r.RouteId = @RouteId;",
        Map,
        Db.P("@TravelDate", travelDate.Date),
        Db.P("@RouteId", routeId));

    public int Create(Route route)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Route
              (RouteType,  Carrier,  RouteCode,  DepartureCity,  ArrivalCity,
               DepartureTime,  ArrivalTime,  DurationMinutes,  IndicativeFarePerSeat,
               SeatCapacity)
        VALUES
              (@RouteType, @Carrier, @RouteCode, @DepartureCity, @ArrivalCity,
               @DepartureTime, @ArrivalTime, @DurationMinutes, @IndicativeFarePerSeat,
               @SeatCapacity);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        BuildParameters(route));

    public void Update(Route route)
    {
        var parameters = new List<SqlParameter> { Db.P("@RouteId", route.RouteId) };
        parameters.AddRange(BuildParameters(route));

        _db.Execute(@"
        UPDATE  dbo.Route
        SET     RouteType             = @RouteType,
                Carrier               = @Carrier,
                RouteCode             = @RouteCode,
                DepartureCity         = @DepartureCity,
                ArrivalCity           = @ArrivalCity,
                DepartureTime         = @DepartureTime,
                ArrivalTime           = @ArrivalTime,
                DurationMinutes       = @DurationMinutes,
                IndicativeFarePerSeat = @IndicativeFarePerSeat,
                SeatCapacity          = @SeatCapacity
        WHERE   RouteId = @RouteId;",
        parameters.ToArray());
    }

    public bool Delete(int routeId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1 FROM dbo.RouteBooking    WHERE RouteId = @RouteId)
                   OR EXISTS (SELECT 1 FROM dbo.TripRequestItem WHERE RouteId = @RouteId)
                 THEN 1 ELSE 0 END;

        IF @HasBookings = 1
            UPDATE dbo.Route SET IsActive = 0 WHERE RouteId = @RouteId;
        ELSE
            DELETE FROM dbo.Route WHERE RouteId = @RouteId;

        SELECT @HasBookings;",
        Db.P("@RouteId", routeId));

    public void Reactivate(int routeId)
        => _db.Execute(@"
        UPDATE dbo.Route SET IsActive = 1 WHERE RouteId = @RouteId;",
        Db.P("@RouteId", routeId));

    private static Route Map(SqlDataReader r) => new Route
    {
        RouteId               = (int)r["RouteId"],
        RouteType             = (string)r["RouteType"],
        Carrier               = (string)r["Carrier"],
        RouteCode             = Db.GetNullableString(r, "RouteCode"),
        DepartureCity         = (string)r["DepartureCity"],
        ArrivalCity           = (string)r["ArrivalCity"],
        DepartureTime         = Db.GetNullable<TimeSpan>(r, "DepartureTime"),
        ArrivalTime           = Db.GetNullable<TimeSpan>(r, "ArrivalTime"),
        DurationMinutes       = Db.GetNullable<int>(r, "DurationMinutes"),
        IndicativeFarePerSeat = (decimal)r["IndicativeFarePerSeat"],
        SeatCapacity          = (int)r["SeatCapacity"],
        SeatsAvailable        = Db.HasColumn(r, "SeatsAvailable")
                                    ? Db.GetNullable<int>(r, "SeatsAvailable") : null,
        IsActive              = (bool)r["IsActive"],
        CreatedDate           = (DateTime)r["CreatedDate"]
    };

    private static SqlParameter[] BuildParameters(Route route) => new[]
    {
        Db.P("@RouteType",             route.RouteType),
        Db.P("@Carrier",               route.Carrier.Trim()),
        Db.P("@RouteCode",             route.RouteCode),
        Db.P("@DepartureCity",         route.DepartureCity.Trim()),
        Db.P("@ArrivalCity",           route.ArrivalCity.Trim()),
        Db.P("@DepartureTime",         route.DepartureTime),
        Db.P("@ArrivalTime",           route.ArrivalTime),
        Db.P("@DurationMinutes",       route.DurationMinutes),
        Db.P("@IndicativeFarePerSeat", route.IndicativeFarePerSeat),
        Db.P("@SeatCapacity",          route.SeatCapacity)
    };
}
