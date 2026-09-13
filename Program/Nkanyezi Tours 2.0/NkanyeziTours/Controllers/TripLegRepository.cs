using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class TripLegRepository
{
    private readonly Db _db;

    public TripLegRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      l.*,
                    prov.ProvinceName,
                    a.Name          AS ResortName,
                    a.City          AS ResortLocation,
                    a.Type          AS ResortType,
                    a.ImageUrl      AS ResortImageUrl,
                    (SELECT COUNT(*) FROM dbo.TripLegRoom AS lr
                     WHERE  lr.TripLegId = l.TripLegId) AS RoomCount,

                    ISNULL((SELECT SUM(rm.Capacity)
                            FROM   dbo.TripLegRoom AS lr
                            JOIN   dbo.Room        AS rm ON rm.RoomId = lr.RoomId
                            WHERE  lr.TripLegId = l.TripLegId), 0) AS RoomsCapacity,

                    (SELECT STRING_AGG(rm.RoomNumber, ', ') WITHIN GROUP (ORDER BY rm.RoomNumber)
                     FROM   dbo.TripLegRoom AS lr
                     JOIN   dbo.Room        AS rm ON rm.RoomId = lr.RoomId
                     WHERE  lr.TripLegId = l.TripLegId) AS RoomNames,

                    ISNULL((SELECT SUM(rm.PricePerNight)
                            FROM   dbo.TripLegRoom AS lr
                            JOIN   dbo.Room        AS rm ON rm.RoomId = lr.RoomId
                            WHERE  lr.TripLegId = l.TripLegId),
                           ISNULL((SELECT MIN(u2.PricePerNight) FROM dbo.Room AS u2
                                   WHERE  u2.AccommodationId    = l.AccommodationId
                                     AND  u2.AvailabilityStatus = 1), 0)) AS ResortPricePerNight,
                    (SELECT COUNT(*)
                     FROM   dbo.TripRequestItem AS tr
                     WHERE  tr.TripLegId = l.TripLegId
                       AND  tr.ItemType  = 'Attraction'
                       AND  tr.Status   <> 'Declined') AS ActivityCount,

                    ISNULL((SELECT SUM(ISNULL(att.Rate, 0) * act.Guests)
                            FROM   dbo.TripRequestItem AS act
                            LEFT JOIN dbo.Attraction   AS att ON att.AttractionId = act.AttractionId
                            WHERE  act.TripLegId = l.TripLegId
                              AND  act.ItemType  = 'Attraction'
                              AND  act.Status   <> 'Declined'), 0) AS ActivityCost,

                    (SELECT COUNT(*)
                     FROM   dbo.TripRequestItem AS tp
                     WHERE  tp.TripLegId = l.TripLegId
                       AND  tp.ItemType IN ('Vehicle','Route')
                       AND  tp.Status   <> 'Declined') AS TransportCount,

                    ISNULL((SELECT SUM(ISNULL(vh.PricePerDay, 0))
                            FROM   dbo.TripRequestItem AS tv
                            JOIN   dbo.Vehicle         AS vh ON vh.VehicleId = tv.VehicleId
                            WHERE  tv.TripLegId = l.TripLegId
                              AND  tv.ItemType  = 'Vehicle'
                              AND  tv.Status   <> 'Declined'), 0)
                    * DATEDIFF(day, l.StartDate, l.EndDate)
                  + ISNULL((SELECT SUM(ISNULL(rr.IndicativeFarePerSeat, 0) * tr2.Guests)
                            FROM   dbo.TripRequestItem AS tr2
                            JOIN   dbo.Route           AS rr ON rr.RouteId = tr2.RouteId
                            WHERE  tr2.TripLegId = l.TripLegId
                              AND  tr2.ItemType  = 'Route'
                              AND  tr2.Status   <> 'Declined'), 0) AS TransportCost
        FROM        dbo.TripLeg       AS l
        LEFT JOIN   dbo.Accommodation AS a    ON a.AccommodationId = l.AccommodationId
        LEFT JOIN   dbo.Province      AS prov ON prov.ProvinceId   = l.ProvinceId";

    public List<TripLeg> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE       l.ItineraryId = @ItineraryId
        ORDER BY    l.LegNumber;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public TripLeg? GetById(int tripLegId)
        => _db.QuerySingle(SelectSql + @"
        WHERE       l.TripLegId = @TripLegId;",
        Map,
        Db.P("@TripLegId", tripLegId));

    public int Append(int itineraryId, int nights, DateTime firstLegStart,
                      string? destination, int? provinceId, int guests)
    {
        if (nights < 1)
            nights = 1;

        var existing = GetByItinerary(itineraryId);

        var legNumber = existing.Count + 1;
        var start     = existing.Count == 0 ? firstLegStart.Date
                                            : existing[existing.Count - 1].EndDate;

        var id = _db.ExecuteInsert(@"
        INSERT INTO dbo.TripLeg
              (ItineraryId,  LegNumber,  Destination,  ProvinceId,  StartDate,  EndDate,  Guests)
        VALUES
              (@ItineraryId, @LegNumber, @Destination, @ProvinceId, @StartDate, @EndDate, @Guests);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@ItineraryId", itineraryId),
        Db.P("@LegNumber",   legNumber),
        Db.P("@Destination", destination),
        Db.P("@ProvinceId",  provinceId),
        Db.P("@StartDate",   start),
        Db.P("@EndDate",     start.AddDays(nights)),
        Db.P("@Guests",      guests));

        SyncItineraryDates(itineraryId);

        return id;
    }

    public void Update(int tripLegId, string? destination, int nights, int guests, string? clientNote)
    {
        if (nights < 1)
            nights = 1;

        var leg = GetById(tripLegId);

        if (leg == null)
            return;

        _db.Execute(@"
        UPDATE  dbo.TripLeg
        SET     Destination = @Destination,
                EndDate     = DATEADD(day, @Nights, StartDate),
                Guests      = @Guests,
                ClientNote  = @ClientNote
        WHERE   TripLegId   = @TripLegId;",
        Db.P("@TripLegId",   tripLegId),
        Db.P("@Destination", destination),
        Db.P("@Nights",      nights),
        Db.P("@Guests",      guests),
        Db.P("@ClientNote",  clientNote));

        Rechain(leg.ItineraryId);
    }

    public void SetResort(int tripLegId, int? accommodationId)
        => _db.Execute(@"
        DELETE  lr
        FROM    dbo.TripLegRoom AS lr
        JOIN    dbo.Room        AS rm ON rm.RoomId = lr.RoomId
        WHERE   lr.TripLegId = @TripLegId
          AND   (@AccommodationId IS NULL OR rm.AccommodationId <> @AccommodationId);

        UPDATE  dbo.TripLeg
        SET     AccommodationId = @AccommodationId
        WHERE   TripLegId       = @TripLegId;",
        Db.P("@TripLegId",       tripLegId),
        Db.P("@AccommodationId", accommodationId));

    public List<int> GetRoomIds(int tripLegId)
        => _db.Query(@"
        SELECT  RoomId
        FROM    dbo.TripLegRoom
        WHERE   TripLegId = @TripLegId;",
        r => (int)r["RoomId"],
        Db.P("@TripLegId", tripLegId));

    public bool AddRoom(int tripLegId, int roomId)
        => _db.Execute(@"
        INSERT INTO dbo.TripLegRoom (TripLegId, RoomId)
        SELECT      l.TripLegId, u.RoomId
        FROM        dbo.TripLeg AS l
        INNER JOIN  dbo.Room    AS u
                ON  u.RoomId          = @RoomId
               AND  u.AccommodationId = l.AccommodationId
        WHERE       l.TripLegId = @TripLegId
          AND       NOT EXISTS (SELECT 1 FROM dbo.TripLegRoom AS x
                                WHERE  x.TripLegId = @TripLegId
                                  AND  x.RoomId    = @RoomId);",
        Db.P("@TripLegId", tripLegId),
        Db.P("@RoomId",    roomId)) > 0;

    public bool RemoveRoom(int tripLegId, int roomId)
        => _db.Execute(@"
        DELETE FROM dbo.TripLegRoom
        WHERE   TripLegId = @TripLegId
          AND   RoomId    = @RoomId;",
        Db.P("@TripLegId", tripLegId),
        Db.P("@RoomId",    roomId)) > 0;

    public void Remove(int tripLegId)
    {
        var leg = GetById(tripLegId);

        if (leg == null)
            return;

        _db.Execute(@"
        UPDATE dbo.TripRequestItem SET TripLegId = NULL WHERE TripLegId = @TripLegId;
        DELETE FROM dbo.TripLeg                          WHERE TripLegId = @TripLegId;",
        Db.P("@TripLegId", tripLegId));

        Renumber(leg.ItineraryId);
        Rechain(leg.ItineraryId);
    }

    private void Renumber(int itineraryId)
        => _db.Execute(@"
        WITH ordered AS (
            SELECT  TripLegId,
                    ROW_NUMBER() OVER (ORDER BY LegNumber) AS NewNumber
            FROM    dbo.TripLeg
            WHERE   ItineraryId = @ItineraryId
        )
        UPDATE  l
        SET     l.LegNumber = o.NewNumber
        FROM    dbo.TripLeg AS l
        JOIN    ordered     AS o ON o.TripLegId = l.TripLegId
        WHERE   l.LegNumber <> o.NewNumber;",
        Db.P("@ItineraryId", itineraryId));

    private void Rechain(int itineraryId)
    {
        var legs = GetByItinerary(itineraryId);

        DateTime? previousEnd = null;

        foreach (var leg in legs)
        {
            if (previousEnd != null && leg.StartDate != previousEnd.Value)
            {
                var nights = Math.Max(leg.Nights, 1);

                _db.Execute(@"
                UPDATE  dbo.TripLeg
                SET     StartDate = @StartDate,
                        EndDate   = DATEADD(day, @Nights, @StartDate)
                WHERE   TripLegId = @TripLegId;",
                Db.P("@TripLegId", leg.TripLegId),
                Db.P("@StartDate", previousEnd.Value),
                Db.P("@Nights",    nights));

                previousEnd = previousEnd.Value.AddDays(nights);
            }
            else
            {
                previousEnd = leg.EndDate;
            }
        }

        SyncItineraryDates(itineraryId);
    }

    private void SyncItineraryDates(int itineraryId)
        => _db.Execute(@"
        UPDATE  i
        SET     i.StartDate = legs.FirstStart,
                i.EndDate   = legs.LastEnd
        FROM    dbo.Itinerary AS i
        CROSS APPLY (
            SELECT  MIN(l.StartDate) AS FirstStart,
                    MAX(l.EndDate)   AS LastEnd
            FROM    dbo.TripLeg AS l
            WHERE   l.ItineraryId = i.ItineraryId
        ) AS legs
        WHERE   i.ItineraryId = @ItineraryId
          AND   legs.FirstStart IS NOT NULL;",
        Db.P("@ItineraryId", itineraryId));

    private static TripLeg Map(SqlDataReader r) => new TripLeg
    {
        TripLegId           = (int)r["TripLegId"],
        ItineraryId         = (int)r["ItineraryId"],
        LegNumber           = (int)r["LegNumber"],
        Destination         = Db.GetNullableString(r, "Destination"),
        ProvinceId          = Db.GetNullable<int>(r, "ProvinceId"),
        ProvinceName        = Db.GetOptionalString(r, "ProvinceName"),
        StartDate           = (DateTime)r["StartDate"],
        EndDate             = (DateTime)r["EndDate"],
        AccommodationId     = Db.GetNullable<int>(r, "AccommodationId"),
        Guests              = (int)r["Guests"],
        ClientNote          = Db.GetNullableString(r, "ClientNote"),
        CreatedDate         = (DateTime)r["CreatedDate"],

        ResortName          = Db.GetOptionalString(r, "ResortName"),
        ResortLocation      = Db.GetOptionalString(r, "ResortLocation"),
        ResortType          = Db.GetOptionalString(r, "ResortType"),
        ResortImageUrl      = Db.GetOptionalString(r, "ResortImageUrl"),
        ResortPricePerNight = (decimal)r["ResortPricePerNight"],
        RoomCount           = (int)r["RoomCount"],
        RoomsCapacity       = (int)r["RoomsCapacity"],
        RoomNames           = Db.GetOptionalString(r, "RoomNames"),
        ActivityCount       = (int)r["ActivityCount"],
        ActivityCost        = (decimal)r["ActivityCost"],
        TransportCount      = (int)r["TransportCount"],
        TransportCost       = (decimal)r["TransportCost"]
    };
}
