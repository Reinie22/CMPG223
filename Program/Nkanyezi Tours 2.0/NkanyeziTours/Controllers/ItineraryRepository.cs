using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class ItineraryRepository
{
    private readonly Db _db;

    public ItineraryRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      i.*,
                    c.Name + ' ' + c.Surname AS ClientName,
                    c.Email                  AS ClientEmail,
                    CASE WHEN a.UserId IS NULL THEN NULL
                         ELSE a.Name + ' ' + a.Surname END AS AgentName
        FROM        dbo.Itinerary AS i
        INNER JOIN  dbo.AppUser   AS c ON c.UserId = i.ClientId
        LEFT JOIN   dbo.AppUser   AS a ON a.UserId = i.AgentId";

    public List<Itinerary> Search(int? clientId = null, int? agentId = null,
                                  string? status = null, string? tripName = null,
                                  DateTime? startsAfter = null, DateTime? startsBefore = null)
        => _db.Query(SelectSql + @"
        WHERE   (@ClientId     IS NULL OR i.ClientId  = @ClientId)
          AND   (@AgentId      IS NULL OR i.AgentId   = @AgentId)
          AND   (@Status       IS NULL OR i.Status    = @Status)
          AND   (@TripName     IS NULL OR i.TripName LIKE @TripName ESCAPE '\')
          AND   (@StartsAfter  IS NULL OR i.StartDate >= @StartsAfter)
          AND   (@StartsBefore IS NULL OR i.StartDate <= @StartsBefore)
        ORDER BY i.CreatedDate DESC;",
        Map,
        Db.P("@ClientId",     clientId),
        Db.P("@AgentId",      agentId),
        Db.P("@Status",       string.IsNullOrWhiteSpace(status) ? null : status),
        Db.Like("@TripName", tripName),
        Db.P("@StartsAfter",  startsAfter?.Date),
        Db.P("@StartsBefore", startsBefore?.Date));

    public List<Itinerary> GetAll() => Search();

    public Itinerary? GetById(int itineraryId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   i.ItineraryId = @ItineraryId;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public List<Itinerary> GetForClient(int clientId) => Search(clientId: clientId);

    public List<Itinerary> GetForAgent(int agentId) => Search(agentId: agentId);

    public List<Itinerary> GetUnassigned()
        => _db.Query(SelectSql + @"
        WHERE   i.AgentId IS NULL
          AND   i.Status <> 'Cancelled'
        ORDER BY i.CreatedDate;",
        Map);

    public Itinerary? GetWithTotals(int itineraryId)
        => _db.QuerySingle(@"
        SELECT      i.*,
                    c.Name + ' ' + c.Surname AS ClientName,
                    c.Email                  AS ClientEmail,
                    CASE WHEN a.UserId IS NULL THEN NULL
                         ELSE a.Name + ' ' + a.Surname END AS AgentName,

                    ISNULL((SELECT SUM(rb.TotalCost) FROM dbo.RoomBooking AS rb
                            WHERE rb.ItineraryId = i.ItineraryId
                              AND rb.Status <> 'Cancelled'), 0)
                  + ISNULL((SELECT SUM(ab.TotalCost) FROM dbo.AttractionBooking AS ab
                            WHERE ab.ItineraryId = i.ItineraryId
                              AND ab.Status <> 'Cancelled'), 0)
                  + ISNULL((SELECT SUM(vb.TotalCost) FROM dbo.VehicleBooking AS vb
                            WHERE vb.ItineraryId = i.ItineraryId
                              AND vb.Status <> 'Cancelled'), 0)
                  + ISNULL((SELECT SUM(rtb.TotalCost) FROM dbo.RouteBooking AS rtb
                            WHERE rtb.ItineraryId = i.ItineraryId
                              AND rtb.Status <> 'Cancelled'), 0)
                        AS BookingsTotal,

                    ISNULL((SELECT SUM(e.Amount) FROM dbo.TripExpense AS e
                            WHERE e.ItineraryId = i.ItineraryId), 0)
                        AS ExpensesTotal

        FROM        dbo.Itinerary AS i
        INNER JOIN  dbo.AppUser   AS c ON c.UserId = i.ClientId
        LEFT JOIN   dbo.AppUser   AS a ON a.UserId = i.AgentId
        WHERE       i.ItineraryId = @ItineraryId;",
        MapWithTotals,
        Db.P("@ItineraryId", itineraryId));

    public List<BookingLine> GetBookingLines(int itineraryId)
        => _db.Query(@"
        SELECT      'Accommodation' AS BookingType,
                    rb.RoomBookingId AS BookingId,
                    acc.Name + ' - room ' + r.RoomNumber AS Description,
                    rb.CheckInDate   AS [Date],
                    rb.TotalCost,
                    rb.Status
        FROM        dbo.RoomBooking   AS rb
        INNER JOIN  dbo.Room          AS r   ON r.RoomId = rb.RoomId
        INNER JOIN  dbo.Accommodation AS acc ON acc.AccommodationId = r.AccommodationId
        WHERE       rb.ItineraryId = @ItineraryId

        UNION ALL

        SELECT      'Attraction',
                    ab.AttractionBookingId,
                    att.Name + ' (' + CAST(ab.Quantity AS NVARCHAR(10)) + ' people)',
                    ab.BookingDate,
                    ab.TotalCost,
                    ab.Status
        FROM        dbo.AttractionBooking AS ab
        INNER JOIN  dbo.Attraction        AS att ON att.AttractionId = ab.AttractionId
        WHERE       ab.ItineraryId = @ItineraryId

        UNION ALL

        SELECT      'Vehicle',
                    vb.VehicleBookingId,
                    ISNULL(v.Make + ' ' + v.Model, v.RegistrationNumber)
                        + ' (' + CAST(vb.TotalDays AS NVARCHAR(10)) + ' days)',
                    vb.StartDate,
                    vb.TotalCost,
                    vb.Status
        FROM        dbo.VehicleBooking AS vb
        INNER JOIN  dbo.Vehicle        AS v ON v.VehicleId = vb.VehicleId
        WHERE       vb.ItineraryId = @ItineraryId

        UNION ALL

        SELECT      'Route',
                    rtb.RouteBookingId,
                    rt.Carrier + ' ' + ISNULL(rt.RouteCode, '')
                        + ' ' + rt.DepartureCity + ' to ' + rt.ArrivalCity
                        + ' (' + CAST(rtb.Seats AS NVARCHAR(10)) + ' seats)',
                    rtb.TravelDate,
                    rtb.TotalCost,
                    rtb.Status
        FROM        dbo.RouteBooking AS rtb
        INNER JOIN  dbo.Route        AS rt ON rt.RouteId = rtb.RouteId
        WHERE       rtb.ItineraryId = @ItineraryId

        ORDER BY    [Date];",
        r => new BookingLine
        {
            BookingType = (string)r["BookingType"],
            BookingId   = (int)r["BookingId"],
            Description = (string)r["Description"],
            Date        = (DateTime)r["Date"],
            TotalCost   = (decimal)r["TotalCost"],
            Status      = (string)r["Status"]
        },
        Db.P("@ItineraryId", itineraryId));

    public decimal GetCurrentTotal(int itineraryId)
        => _db.Scalar<decimal>(@"
        SELECT  ISNULL((SELECT SUM(TotalCost) FROM dbo.RoomBooking
                        WHERE ItineraryId = @ItineraryId AND Status <> 'Cancelled'), 0)
              + ISNULL((SELECT SUM(TotalCost) FROM dbo.AttractionBooking
                        WHERE ItineraryId = @ItineraryId AND Status <> 'Cancelled'), 0)
              + ISNULL((SELECT SUM(TotalCost) FROM dbo.VehicleBooking
                        WHERE ItineraryId = @ItineraryId AND Status <> 'Cancelled'), 0)
              + ISNULL((SELECT SUM(TotalCost) FROM dbo.RouteBooking
                        WHERE ItineraryId = @ItineraryId AND Status <> 'Cancelled'), 0)
              + ISNULL((SELECT SUM(Amount)    FROM dbo.TripExpense
                        WHERE ItineraryId = @ItineraryId), 0);",
        Db.P("@ItineraryId", itineraryId));

    public bool BelongsToClient(int itineraryId, int clientId)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.Itinerary
        WHERE   ItineraryId = @ItineraryId
          AND   ClientId    = @ClientId;",
        Db.P("@ItineraryId", itineraryId),
        Db.P("@ClientId",    clientId)) > 0;

    public int Create(Itinerary trip)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Itinerary
              (ClientId,  AgentId,  TripName,  StartDate,  EndDate,
               TotalBudget,  TripComments,  Status)
        VALUES
              (@ClientId, @AgentId, @TripName, @StartDate, @EndDate,
               @TotalBudget, @TripComments, @Status);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@ClientId",     trip.ClientId),
        Db.P("@AgentId",      trip.AgentId),
        Db.P("@TripName",     trip.TripName.Trim()),
        Db.P("@StartDate",    trip.StartDate?.Date),
        Db.P("@EndDate",      trip.EndDate?.Date),
        Db.P("@TotalBudget",  trip.TotalBudget),
        Db.P("@TripComments", trip.TripComments),
        Db.P("@Status",       trip.Status));

    public void Update(Itinerary trip)
        => _db.Execute(@"
        UPDATE  dbo.Itinerary
        SET     ClientId     = @ClientId,
                AgentId      = @AgentId,
                TripName     = @TripName,
                StartDate    = @StartDate,
                EndDate      = @EndDate,
                TotalBudget  = @TotalBudget,
                TripComments = @TripComments,
                Status       = @Status
        WHERE   ItineraryId = @ItineraryId;",
        Db.P("@ItineraryId",  trip.ItineraryId),
        Db.P("@ClientId",     trip.ClientId),
        Db.P("@AgentId",      trip.AgentId),
        Db.P("@TripName",     trip.TripName.Trim()),
        Db.P("@StartDate",    trip.StartDate?.Date),
        Db.P("@EndDate",      trip.EndDate?.Date),
        Db.P("@TotalBudget",  trip.TotalBudget),
        Db.P("@TripComments", trip.TripComments),
        Db.P("@Status",       trip.Status));

    public void AssignToAgent(int itineraryId, int? agentId)
        => _db.Execute(@"
        UPDATE  dbo.Itinerary
        SET     AgentId = @AgentId
        WHERE   ItineraryId = @ItineraryId;",
        Db.P("@ItineraryId", itineraryId),
        Db.P("@AgentId",     agentId));

    public void SetStatus(int itineraryId, string status)
        => _db.Execute(@"
        UPDATE  dbo.Itinerary
        SET     Status = @Status
        WHERE   ItineraryId = @ItineraryId;",
        Db.P("@ItineraryId", itineraryId),
        Db.P("@Status",      status));

    public bool Delete(int itineraryId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1 FROM dbo.RoomBooking       WHERE ItineraryId = @ItineraryId)
                   OR EXISTS (SELECT 1 FROM dbo.AttractionBooking WHERE ItineraryId = @ItineraryId)
                   OR EXISTS (SELECT 1 FROM dbo.VehicleBooking    WHERE ItineraryId = @ItineraryId)
                   OR EXISTS (SELECT 1 FROM dbo.RouteBooking      WHERE ItineraryId = @ItineraryId)
                   OR EXISTS (SELECT 1 FROM dbo.TripExpense       WHERE ItineraryId = @ItineraryId)
                 THEN 1 ELSE 0 END;

        IF @HasBookings = 1
            UPDATE dbo.Itinerary SET Status = 'Cancelled' WHERE ItineraryId = @ItineraryId;
        ELSE
            DELETE FROM dbo.Itinerary WHERE ItineraryId = @ItineraryId;

        SELECT @HasBookings;",
        Db.P("@ItineraryId", itineraryId));

    private static Itinerary Map(SqlDataReader r) => new Itinerary
    {
        ItineraryId  = (int)r["ItineraryId"],
        ClientId     = (int)r["ClientId"],
        ClientName   = Db.GetOptionalString(r, "ClientName"),
        ClientEmail  = Db.GetOptionalString(r, "ClientEmail"),
        AgentId      = Db.GetNullable<int>(r, "AgentId"),
        AgentName    = Db.GetOptionalString(r, "AgentName"),
        TripName     = (string)r["TripName"],
        StartDate    = Db.GetNullable<DateTime>(r, "StartDate"),
        EndDate      = Db.GetNullable<DateTime>(r, "EndDate"),
        TotalBudget  = (decimal)r["TotalBudget"],
        TripComments = Db.GetNullableString(r, "TripComments"),
        Status       = (string)r["Status"],
        CreatedDate  = (DateTime)r["CreatedDate"]
    };

    private static Itinerary MapWithTotals(SqlDataReader r)
    {
        var trip = Map(r);
        trip.BookingsTotal = (decimal)r["BookingsTotal"];
        trip.ExpensesTotal = (decimal)r["ExpensesTotal"];
        return trip;
    }
}
