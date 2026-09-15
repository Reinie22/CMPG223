using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class TripRequestRepository
{
    private readonly Db _db;

    public TripRequestRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      tr.*,

                    COALESCE(acc.Name,
                             att.Name,
                             CONCAT(veh.Make, ' ', veh.Model),
                             CONCAT(rt.Carrier, ' ', rt.RouteCode)) AS ItemName,

                    COALESCE(acc.City,
                             att.City,
                             veh.Location,
                             CONCAT(rt.DepartureCity, ' to ', rt.ArrivalCity)) AS ItemLocation,

                    COALESCE((SELECT MIN(u.PricePerNight) FROM dbo.Room AS u
                              WHERE u.AccommodationId    = tr.AccommodationId
                                AND u.AvailabilityStatus = 1),
                             att.Rate,
                             veh.PricePerDay,
                             rt.IndicativeFarePerSeat, 0)                     AS GuidePrice,

                    rt.RouteType                                              AS RouteType
        FROM        dbo.TripRequestItem AS tr
        LEFT JOIN   dbo.Accommodation   AS acc ON acc.AccommodationId = tr.AccommodationId
        LEFT JOIN   dbo.Attraction      AS att ON att.AttractionId    = tr.AttractionId
        LEFT JOIN   dbo.Vehicle         AS veh ON veh.VehicleId       = tr.VehicleId
        LEFT JOIN   dbo.Route           AS rt  ON rt.RouteId          = tr.RouteId";

    public List<TripRequestItem> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE       tr.ItineraryId = @ItineraryId
        ORDER BY    tr.CreatedDate;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public List<TripRequestItem> GetPending(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE       tr.ItineraryId = @ItineraryId
          AND       tr.Status = 'Requested'
        ORDER BY    tr.CreatedDate;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public TripRequestItem? GetById(int tripRequestItemId)
        => _db.QuerySingle(SelectSql + @"
        WHERE       tr.TripRequestItemId = @Id;",
        Map,
        Db.P("@Id", tripRequestItemId));

    public int CountPendingForAgent(int agentId)
        => _db.Scalar<int>(@"
        SELECT      COUNT(*)
        FROM        dbo.TripRequestItem AS tr
        INNER JOIN  dbo.Itinerary       AS i ON i.ItineraryId = tr.ItineraryId
        WHERE       tr.Status = 'Requested'
          AND       i.AgentId = @AgentId;",
        Db.P("@AgentId", agentId));

    public int Add(TripRequestItem item)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.TripRequestItem
              (ItineraryId,  TripLegId,  ItemType,  AccommodationId,  AttractionId,
               VehicleId,  RouteId,
               StartDate,  EndDate,  Guests,  ClientNote)
        VALUES
              (@ItineraryId, @TripLegId, @ItemType, @AccommodationId, @AttractionId,
               @VehicleId, @RouteId,
               @StartDate, @EndDate, @Guests, @ClientNote);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@ItineraryId",     item.ItineraryId),
        Db.P("@TripLegId",       item.TripLegId),
        Db.P("@ItemType",        item.ItemType),
        Db.P("@AccommodationId", item.AccommodationId),
        Db.P("@AttractionId",    item.AttractionId),
        Db.P("@VehicleId",       item.VehicleId),
        Db.P("@RouteId",         item.RouteId),
        Db.P("@StartDate",       item.StartDate?.Date),
        Db.P("@EndDate",         item.EndDate?.Date),
        Db.P("@Guests",          item.Guests),
        Db.P("@ClientNote",      item.ClientNote));

    public bool RemoveIfPending(int tripRequestItemId, int itineraryId)
        => _db.Execute(@"
        DELETE FROM dbo.TripRequestItem
        WHERE   TripRequestItemId = @Id
          AND   ItineraryId       = @ItineraryId
          AND   Status            = 'Requested';",
        Db.P("@Id",          tripRequestItemId),
        Db.P("@ItineraryId", itineraryId)) > 0;

    public bool Decide(int tripRequestItemId, string status, string? agentNote)
        => _db.Execute(@"
        UPDATE  dbo.TripRequestItem
        SET     Status    = @Status,
                AgentNote = @AgentNote
        WHERE   TripRequestItemId = @Id
          AND   Status = 'Requested';",
        Db.P("@Id",        tripRequestItemId),
        Db.P("@Status",    status),
        Db.P("@AgentNote", agentNote)) > 0;

    private static TripRequestItem Map(SqlDataReader r) => new TripRequestItem
    {
        TripRequestItemId = (int)r["TripRequestItemId"],
        ItineraryId       = (int)r["ItineraryId"],
        TripLegId         = Db.GetNullable<int>(r, "TripLegId"),
        ItemType          = (string)r["ItemType"],
        AccommodationId   = Db.GetNullable<int>(r, "AccommodationId"),
        AttractionId      = Db.GetNullable<int>(r, "AttractionId"),
        VehicleId         = Db.GetNullable<int>(r, "VehicleId"),
        RouteId           = Db.GetNullable<int>(r, "RouteId"),
        StartDate         = Db.GetNullable<DateTime>(r, "StartDate"),
        EndDate           = Db.GetNullable<DateTime>(r, "EndDate"),
        Guests            = (int)r["Guests"],
        ClientNote        = Db.GetNullableString(r, "ClientNote"),
        Status            = (string)r["Status"],
        AgentNote         = Db.GetNullableString(r, "AgentNote"),
        CreatedDate       = (DateTime)r["CreatedDate"],

        ItemName          = Db.GetOptionalString(r, "ItemName") ?? "",
        ItemLocation      = Db.GetOptionalString(r, "ItemLocation"),
        GuidePrice        = (decimal)r["GuidePrice"],
        RouteType         = Db.GetOptionalString(r, "RouteType")
    };
}
