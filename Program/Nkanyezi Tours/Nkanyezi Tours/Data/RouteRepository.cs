using Microsoft.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// Everything the system can do to flights and bus shuttles.
///
/// One repository serves both, because a flight and a shuttle are the same
/// thing in the database: a journey between two cities priced per seat. The
/// RouteType column is the only difference, so whoever builds the flight
/// screens gets the shuttle screens for nothing.
/// </summary>
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

    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

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

    /// <summary>
    /// Filtered list. The portal uses this to answer "roughly what does
    /// Johannesburg to New York cost", which is the enquiry that ties up the
    /// agents' telephones all day.
    /// </summary>
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

    /// <summary>Just the flights, or just the shuttles. Use RouteTypes.</summary>
    public List<Route> GetByType(string routeType) => Search(routeType: routeType);

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

    public int Create(Route route)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Route
              (RouteType,  Carrier,  RouteCode,  DepartureCity,  ArrivalCity,
               DepartureTime,  ArrivalTime,  DurationMinutes,  IndicativeFarePerSeat)
        VALUES
              (@RouteType, @Carrier, @RouteCode, @DepartureCity, @ArrivalCity,
               @DepartureTime, @ArrivalTime, @DurationMinutes, @IndicativeFarePerSeat);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        BuildParameters(route));

    /// <summary>
    /// Saves changes to a route.
    ///
    /// Changing the indicative fare affects new bookings only. Existing
    /// bookings keep the fare that was actually paid, because that figure was
    /// copied onto the booking row.
    /// </summary>
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
                IndicativeFarePerSeat = @IndicativeFarePerSeat
        WHERE   RouteId = @RouteId;",
        parameters.ToArray());
    }

    /// <summary>
    /// Removes the route, or deactivates it when seats have been booked on it.
    /// Returns true when it was deactivated rather than deleted.
    /// </summary>
    public bool Delete(int routeId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1 FROM dbo.RouteBooking WHERE RouteId = @RouteId)
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

    // -------------------------------------------------------------------------

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
        Db.P("@IndicativeFarePerSeat", route.IndicativeFarePerSeat)
    };
}
