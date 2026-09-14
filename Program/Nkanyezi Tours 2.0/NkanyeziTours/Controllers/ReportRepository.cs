using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class ReportRepository
{
    private readonly Db _db;

    public ReportRepository(Db db)
    {
        _db = db;
    }

    private const string TripsCte = @"
        Trips AS (
            SELECT      i.ItineraryId,
                        i.TripName,
                        i.ClientId,
                        c.Name + ' ' + c.Surname AS ClientName,
                        CASE WHEN a.UserId IS NULL THEN NULL
                             ELSE a.Name + ' ' + a.Surname END AS AgentName,
                        COALESCE(i.StartDate, CAST(i.CreatedDate AS DATE)) AS TripDate,
                        i.Status,
                        i.TotalBudget,
                        CASE WHEN EXISTS (SELECT 1 FROM dbo.RoomBooking AS b
                                          WHERE b.ItineraryId = i.ItineraryId AND b.Status <> 'Cancelled')
                               OR EXISTS (SELECT 1 FROM dbo.AttractionBooking AS b
                                          WHERE b.ItineraryId = i.ItineraryId AND b.Status <> 'Cancelled')
                               OR EXISTS (SELECT 1 FROM dbo.VehicleBooking AS b
                                          WHERE b.ItineraryId = i.ItineraryId AND b.Status <> 'Cancelled')
                               OR EXISTS (SELECT 1 FROM dbo.RouteBooking AS b
                                          WHERE b.ItineraryId = i.ItineraryId AND b.Status <> 'Cancelled')
                             THEN 1 ELSE 0 END AS IsBooked
            FROM        dbo.Itinerary AS i
            INNER JOIN  dbo.AppUser   AS c ON c.UserId = i.ClientId
            LEFT JOIN   dbo.AppUser   AS a ON a.UserId = i.AgentId
            WHERE       i.Status IN ('Confirmed', 'Completed')
              AND       COALESCE(i.StartDate, CAST(i.CreatedDate AS DATE)) BETWEEN @FromDate AND @ToDate
              AND       (@ClientId IS NULL OR i.ClientId = @ClientId)
        )";

    public List<ReportTripValue> GetTripValues(DateTime fromDate, DateTime toDate, int? clientId = null)
        => _db.Query(@"
        WITH " + TripsCte + @",
        LegCosts AS (
            SELECT      l.ItineraryId,
                        ISNULL(rooms.Price, ISNULL(cheapest.Price, 0))
                            * DATEDIFF(DAY, l.StartDate, l.EndDate)             AS Accommodation,
                        ISNULL(act.Cost, 0)                                     AS Activities,
                        ISNULL(car.PerDay, 0) * DATEDIFF(DAY, l.StartDate, l.EndDate) AS CarRental,
                        ISNULL(fare.Cost, 0)                                    AS FlightsAndBuses
            FROM        dbo.TripLeg AS l
            INNER JOIN  Trips       AS t ON t.ItineraryId = l.ItineraryId AND t.IsBooked = 0
            OUTER APPLY (SELECT SUM(rm.PricePerNight) AS Price
                         FROM   dbo.TripLegRoom AS lr
                         JOIN   dbo.Room        AS rm ON rm.RoomId = lr.RoomId
                         WHERE  lr.TripLegId = l.TripLegId) AS rooms
            OUTER APPLY (SELECT MIN(rm.PricePerNight) AS Price
                         FROM   dbo.Room AS rm
                         WHERE  rm.AccommodationId    = l.AccommodationId
                           AND  rm.AvailabilityStatus = 1) AS cheapest
            OUTER APPLY (SELECT SUM(ISNULL(att.Rate, 0) * x.Guests) AS Cost
                         FROM   dbo.TripRequestItem AS x
                         LEFT JOIN dbo.Attraction   AS att ON att.AttractionId = x.AttractionId
                         WHERE  x.TripLegId = l.TripLegId
                           AND  x.ItemType  = 'Attraction'
                           AND  x.Status   <> 'Declined') AS act
            OUTER APPLY (SELECT SUM(ISNULL(v.PricePerDay, 0)) AS PerDay
                         FROM   dbo.TripRequestItem AS x
                         JOIN   dbo.Vehicle         AS v ON v.VehicleId = x.VehicleId
                         WHERE  x.TripLegId = l.TripLegId
                           AND  x.ItemType  = 'Vehicle'
                           AND  x.Status   <> 'Declined') AS car
            OUTER APPLY (SELECT SUM(ISNULL(r.IndicativeFarePerSeat, 0) * x.Guests) AS Cost
                         FROM   dbo.TripRequestItem AS x
                         JOIN   dbo.Route           AS r ON r.RouteId = x.RouteId
                         WHERE  x.TripLegId = l.TripLegId
                           AND  x.ItemType  = 'Route'
                           AND  x.Status   <> 'Declined') AS fare
        ),
        Quoted AS (
            SELECT      ItineraryId,
                        SUM(Accommodation)   AS Accommodation,
                        SUM(Activities)      AS Activities,
                        SUM(CarRental)       AS CarRental,
                        SUM(FlightsAndBuses) AS FlightsAndBuses
            FROM        LegCosts
            GROUP BY    ItineraryId
        )
        SELECT      t.ItineraryId,
                    t.TripName,
                    t.ClientId,
                    t.ClientName,
                    t.AgentName,
                    t.TripDate,
                    t.Status,
                    t.TotalBudget,
                    CASE WHEN t.IsBooked = 1 THEN 'Booked' ELSE 'Quoted' END AS Source,
                    CASE WHEN t.IsBooked = 1
                         THEN ISNULL((SELECT SUM(b.TotalCost) FROM dbo.RoomBooking AS b
                                      WHERE b.ItineraryId = t.ItineraryId AND b.Status <> 'Cancelled'), 0)
                         ELSE ISNULL(q.Accommodation, 0) END AS Accommodation,
                    CASE WHEN t.IsBooked = 1
                         THEN ISNULL((SELECT SUM(b.TotalCost) FROM dbo.AttractionBooking AS b
                                      WHERE b.ItineraryId = t.ItineraryId AND b.Status <> 'Cancelled'), 0)
                         ELSE ISNULL(q.Activities, 0) END AS Activities,
                    CASE WHEN t.IsBooked = 1
                         THEN ISNULL((SELECT SUM(b.TotalCost) FROM dbo.VehicleBooking AS b
                                      WHERE b.ItineraryId = t.ItineraryId AND b.Status <> 'Cancelled'), 0)
                         ELSE ISNULL(q.CarRental, 0) END AS CarRental,
                    CASE WHEN t.IsBooked = 1
                         THEN ISNULL((SELECT SUM(b.TotalCost) FROM dbo.RouteBooking AS b
                                      WHERE b.ItineraryId = t.ItineraryId AND b.Status <> 'Cancelled'), 0)
                         ELSE ISNULL(q.FlightsAndBuses, 0) END AS FlightsAndBuses,
                    ISNULL((SELECT SUM(e.Amount) FROM dbo.TripExpense AS e
                            WHERE e.ItineraryId = t.ItineraryId), 0) AS Expenses
        FROM        Trips  AS t
        LEFT JOIN   Quoted AS q ON q.ItineraryId = t.ItineraryId
        ORDER BY    t.TripDate, t.TripName;",
        r => new ReportTripValue
        {
            ItineraryId     = (int)r["ItineraryId"],
            TripName        = (string)r["TripName"],
            ClientId        = (int)r["ClientId"],
            ClientName      = (string)r["ClientName"],
            AgentName       = Db.GetNullableString(r, "AgentName"),
            TripDate        = (DateTime)r["TripDate"],
            Status          = (string)r["Status"],
            TotalBudget     = Convert.ToDecimal(r["TotalBudget"]),
            Source          = (string)r["Source"],
            Accommodation   = Convert.ToDecimal(r["Accommodation"]),
            Activities      = Convert.ToDecimal(r["Activities"]),
            CarRental       = Convert.ToDecimal(r["CarRental"]),
            FlightsAndBuses = Convert.ToDecimal(r["FlightsAndBuses"]),
            Expenses        = Convert.ToDecimal(r["Expenses"])
        },
        Db.P("@FromDate", fromDate.Date),
        Db.P("@ToDate",   toDate.Date),
        Db.P("@ClientId", clientId));

    public List<ReportProvinceCount> GetTripsByProvince(DateTime fromDate, DateTime toDate)
        => _db.Query(@"
        WITH " + TripsCte + @",
        TripProvinces AS (
            SELECT  l.ItineraryId, l.ProvinceId
            FROM    dbo.TripLeg AS l
            JOIN    Trips       AS t ON t.ItineraryId = l.ItineraryId AND t.IsBooked = 0
            UNION
            SELECT  l.ItineraryId, acc.ProvinceId
            FROM    dbo.TripLeg       AS l
            JOIN    Trips             AS t   ON t.ItineraryId       = l.ItineraryId AND t.IsBooked = 0
            JOIN    dbo.Accommodation AS acc ON acc.AccommodationId = l.AccommodationId
            UNION
            SELECT  rb.ItineraryId, acc.ProvinceId
            FROM    dbo.RoomBooking   AS rb
            JOIN    Trips             AS t   ON t.ItineraryId       = rb.ItineraryId AND t.IsBooked = 1
            JOIN    dbo.Room          AS rm  ON rm.RoomId           = rb.RoomId
            JOIN    dbo.Accommodation AS acc ON acc.AccommodationId = rm.AccommodationId
            WHERE   rb.Status <> 'Cancelled'
            UNION
            SELECT  ab.ItineraryId, att.ProvinceId
            FROM    dbo.AttractionBooking AS ab
            JOIN    Trips                 AS t   ON t.ItineraryId    = ab.ItineraryId AND t.IsBooked = 1
            JOIN    dbo.Attraction        AS att ON att.AttractionId = ab.AttractionId
            WHERE   ab.Status <> 'Cancelled'
        )
        SELECT      p.ProvinceName,
                    COUNT(DISTINCT tp.ItineraryId) AS Trips
        FROM        dbo.Province  AS p
        LEFT JOIN   TripProvinces AS tp ON tp.ProvinceId = p.ProvinceId
        GROUP BY    p.ProvinceName
        ORDER BY    Trips DESC, p.ProvinceName;",
        r => new ReportProvinceCount
        {
            ProvinceName = (string)r["ProvinceName"],
            Trips        = (int)r["Trips"]
        },
        Db.P("@FromDate", fromDate.Date),
        Db.P("@ToDate",   toDate.Date),
        Db.P("@ClientId", null));

    public List<ReportActivityCount> GetTopActivities(DateTime fromDate, DateTime toDate, int top = 10)
        => _db.Query(@"
        WITH " + TripsCte + @",
        ActivityLines AS (
            SELECT  ab.AttractionId, ab.ItineraryId, ab.Quantity AS People
            FROM    dbo.AttractionBooking AS ab
            JOIN    Trips                 AS t ON t.ItineraryId = ab.ItineraryId AND t.IsBooked = 1
            WHERE   ab.Status <> 'Cancelled'
            UNION ALL
            SELECT  x.AttractionId, x.ItineraryId, x.Guests
            FROM    dbo.TripRequestItem AS x
            JOIN    Trips               AS t ON t.ItineraryId = x.ItineraryId AND t.IsBooked = 0
            WHERE   x.ItemType     = 'Attraction'
              AND   x.Status      <> 'Declined'
              AND   x.AttractionId IS NOT NULL
        )
        SELECT TOP (@Top)
                    att.Name                        AS ActivityName,
                    cat.CategoryName                AS ActivityType,
                    p.ProvinceName,
                    COUNT(DISTINCT al.ItineraryId)  AS Trips,
                    SUM(al.People)                  AS People
        FROM        ActivityLines          AS al
        JOIN        dbo.Attraction         AS att ON att.AttractionId = al.AttractionId
        LEFT JOIN   dbo.AttractionCategory AS cat ON cat.CategoryId   = att.CategoryId
        LEFT JOIN   dbo.Province           AS p   ON p.ProvinceId     = att.ProvinceId
        GROUP BY    att.AttractionId, att.Name, cat.CategoryName, p.ProvinceName
        ORDER BY    Trips DESC, People DESC, att.Name;",
        r => new ReportActivityCount
        {
            ActivityName = (string)r["ActivityName"],
            ActivityType = Db.GetNullableString(r, "ActivityType"),
            ProvinceName = Db.GetNullableString(r, "ProvinceName"),
            Trips        = (int)r["Trips"],
            People       = (int)r["People"]
        },
        Db.P("@FromDate", fromDate.Date),
        Db.P("@ToDate",   toDate.Date),
        Db.P("@ClientId", null),
        Db.P("@Top",      top));
}
