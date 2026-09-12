using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// What a client has asked for on their own draft trip, and what the agent has
/// done about it.
///
/// A request is not a booking. Nothing in here touches capacity or money -
/// that happens when the agent turns an approved request into a real booking
/// through RoomBookingRepository or AttractionBookingRepository. See the
/// comment at the top of database/05-trip-request-items.sql for why it is built
/// this way round.
/// </summary>
public class TripRequestRepository
{
    private readonly Db _db;

    public TripRequestRepository(Db db)
    {
        _db = db;
    }

    /// <summary>
    /// The column list shared by every read.
    ///
    /// One row can point at either a hotel or an attraction, never both, so
    /// both joins are LEFT and COALESCE picks whichever side is filled in. That
    /// keeps one query for both kinds instead of two queries the pages would
    /// then have to merge and sort themselves.
    /// </summary>
    private const string SelectSql = @"
        SELECT      tr.*,
                    COALESCE(acc.Name, att.Name)          AS ItemName,
                    COALESCE(acc.City, att.City)                    AS ItemLocation,
                    COALESCE(acc.PricePerNight, att.Rate, 0)        AS GuidePrice
        FROM        dbo.TripRequestItem AS tr
        LEFT JOIN   dbo.Accommodation   AS acc ON acc.AccommodationId = tr.AccommodationId
        LEFT JOIN   dbo.Attraction      AS att ON att.AttractionId    = tr.AttractionId";

    /// <summary>Everything on one trip, oldest first, both kinds together.</summary>
    public List<TripRequestItem> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE       tr.ItineraryId = @ItineraryId
        ORDER BY    tr.CreatedDate;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    /// <summary>Only the ones still waiting on an agent.</summary>
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

    /// <summary>How many trips have something waiting, for the agent's queue badge.</summary>
    public int CountPendingForAgent(int agentId)
        => _db.Scalar<int>(@"
        SELECT      COUNT(*)
        FROM        dbo.TripRequestItem AS tr
        INNER JOIN  dbo.Itinerary       AS i ON i.ItineraryId = tr.ItineraryId
        WHERE       tr.Status = 'Requested'
          AND       i.AgentId = @AgentId;",
        Db.P("@AgentId", agentId));

    /// <summary>
    /// Adds one request. Status is left to the column default, so a client
    /// cannot create something already approved by passing a status in.
    /// </summary>
    public int Add(TripRequestItem item)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.TripRequestItem
              (ItineraryId,  ItemType,  AccommodationId,  AttractionId,
               StartDate,  EndDate,  Guests,  ClientNote)
        VALUES
              (@ItineraryId, @ItemType, @AccommodationId, @AttractionId,
               @StartDate, @EndDate, @Guests, @ClientNote);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@ItineraryId",     item.ItineraryId),
        Db.P("@ItemType",        item.ItemType),
        Db.P("@AccommodationId", item.AccommodationId),
        Db.P("@AttractionId",    item.AttractionId),
        Db.P("@StartDate",       item.StartDate?.Date),
        Db.P("@EndDate",         item.EndDate?.Date),
        Db.P("@Guests",          item.Guests),
        Db.P("@ClientNote",      item.ClientNote));

    /// <summary>
    /// The client takes something off their own draft.
    ///
    /// Only while it is still 'Requested' - once an agent has approved it there
    /// is a real booking behind it, and that has to be cancelled properly on the
    /// booking itself rather than made to vanish from here.
    /// </summary>
    public bool RemoveIfPending(int tripRequestItemId, int itineraryId)
        => _db.Execute(@"
        DELETE FROM dbo.TripRequestItem
        WHERE   TripRequestItemId = @Id
          AND   ItineraryId       = @ItineraryId
          AND   Status            = 'Requested';",
        Db.P("@Id",          tripRequestItemId),
        Db.P("@ItineraryId", itineraryId)) > 0;

    /// <summary>
    /// The agent's decision.
    ///
    /// "AND Status = 'Requested'" is the same guard as ClaimByAgent: if two
    /// people have the trip open, the second decision changes no rows and
    /// returns false instead of overwriting the first.
    /// </summary>
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

    // -------------------------------------------------------------------------

    private static TripRequestItem Map(SqlDataReader r) => new TripRequestItem
    {
        TripRequestItemId = (int)r["TripRequestItemId"],
        ItineraryId       = (int)r["ItineraryId"],
        ItemType          = (string)r["ItemType"],
        AccommodationId   = Db.GetNullable<int>(r, "AccommodationId"),
        AttractionId      = Db.GetNullable<int>(r, "AttractionId"),
        StartDate         = Db.GetNullable<DateTime>(r, "StartDate"),
        EndDate           = Db.GetNullable<DateTime>(r, "EndDate"),
        Guests            = (int)r["Guests"],
        ClientNote        = Db.GetNullableString(r, "ClientNote"),
        Status            = (string)r["Status"],
        AgentNote         = Db.GetNullableString(r, "AgentNote"),
        CreatedDate       = (DateTime)r["CreatedDate"],

        ItemName          = Db.GetOptionalString(r, "ItemName") ?? "",
        ItemLocation      = Db.GetOptionalString(r, "ItemLocation"),
        GuidePrice        = (decimal)r["GuidePrice"]
    };
}
