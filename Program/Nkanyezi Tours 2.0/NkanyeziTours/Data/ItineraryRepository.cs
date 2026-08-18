using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// Everything the system can do to trips.
///
/// The centre of the system. Every booking and every expense points at an
/// itinerary, and this class is where they get added up against the budget.
///
/// Note that this repository does NOT decide whether a booking is allowed, or
/// whether a trip is over budget. It fetches and saves rows and works out
/// totals. The rules live in a service class, so that six people building
/// screens cannot each implement the same rule slightly differently.
/// </summary>
public class ItineraryRepository
{
    private readonly Db _db;

    public ItineraryRepository(Db db)
    {
        _db = db;
    }

    /// <summary>
    /// The column list shared by every read.
    ///
    /// AppUser is joined twice, once as the client and once as the agent, which
    /// is why each join needs its own alias. INNER JOIN for the client because
    /// every trip has one; LEFT JOIN for the agent because a trip submitted
    /// through the portal has no agent until somebody claims it. Using INNER
    /// JOIN there would make the unassigned queue permanently empty - and that
    /// bug looks exactly like "the feature does not work".
    /// </summary>
    private const string SelectSql = @"
        SELECT      i.*,
                    c.Name + ' ' + c.Surname AS ClientName,
                    c.Email                  AS ClientEmail,
                    CASE WHEN a.UserId IS NULL THEN NULL
                         ELSE a.Name + ' ' + a.Surname END AS AgentName
        FROM        dbo.Itinerary AS i
        INNER JOIN  dbo.AppUser   AS c ON c.UserId = i.ClientId
        LEFT JOIN   dbo.AppUser   AS a ON a.UserId = i.AgentId";

    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

    /// <summary>
    /// The trip list, with every filter optional.
    ///
    /// This one method serves the whole list screen: all trips for a manager,
    /// an agent's own trips, one client's trips, and the status filter. Passing
    /// null for a filter drops it out of the query.
    /// </summary>
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

    /// <summary>One client's own trips, for the portal.</summary>
    public List<Itinerary> GetForClient(int clientId) => Search(clientId: clientId);

    /// <summary>One agent's trips, for their dashboard.</summary>
    public List<Itinerary> GetForAgent(int agentId) => Search(agentId: agentId);

    /// <summary>
    /// Trips submitted from the portal that nobody has picked up.
    ///
    /// This queue is the reason AgentId allows NULL. Without a screen showing
    /// it, a client can plan a trip and no agent would ever know about it.
    /// Cancelled trips are left out - there is nothing to claim.
    /// </summary>
    public List<Itinerary> GetUnassigned()
        => _db.Query(SelectSql + @"
        WHERE   i.AgentId IS NULL
          AND   i.Status <> 'Cancelled'
        ORDER BY i.CreatedDate;",
        Map);

    /// <summary>
    /// One trip with its booking and expense totals filled in, for the detail
    /// screen and the trip cost summary.
    ///
    /// Each total is a subquery in the SELECT list rather than five joins.
    /// Joining all four booking tables at once multiplies the rows together -
    /// a trip with two rooms and three attractions would produce six rows and
    /// every total would come out badly wrong. Subqueries each return one
    /// number and cannot interfere with each other.
    ///
    /// ISNULL around each SUM because a SUM over no rows is NULL, not zero, and
    /// a NULL would make the whole grand total NULL.
    ///
    /// Cancelled bookings are excluded everywhere. A cancelled booking costs
    /// the client nothing and must not eat their budget.
    /// </summary>
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

    /// <summary>
    /// Every booking on a trip - rooms, attractions, cars and seats - as one
    /// list sorted by date, for the itinerary detail table.
    ///
    /// UNION ALL stitches the four tables into one result. Each SELECT has to
    /// produce the same columns in the same order and the same types, which is
    /// why every branch names its columns identically.
    ///
    /// UNION ALL rather than UNION: UNION removes duplicate rows, which costs a
    /// sort and could genuinely hide a real booking - a client legitimately
    /// books the same attraction twice on the same day for two groups. ALL
    /// keeps everything.
    /// </summary>
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

    /// <summary>
    /// What the trip costs so far, bookings and expenses together.
    ///
    /// The budget check calls this before saving a new booking, adds the cost
    /// of the booking about to be made, and compares the two. Cheaper than
    /// loading the whole trip when only the number is needed.
    /// </summary>
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

    /// <summary>
    /// True when this trip belongs to this client.
    ///
    /// Every portal page must call this before showing a trip. Without it, a
    /// client can change the number in the address bar and read somebody else's
    /// itinerary - the most common access-control hole in a web application,
    /// and one that is invisible until somebody tries it.
    /// </summary>
    public bool BelongsToClient(int itineraryId, int clientId)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.Itinerary
        WHERE   ItineraryId = @ItineraryId
          AND   ClientId    = @ClientId;",
        Db.P("@ItineraryId", itineraryId),
        Db.P("@ClientId",    clientId)) > 0;

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

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

    /// <summary>
    /// An agent claims an unassigned trip from the queue.
    ///
    /// The "AND AgentId IS NULL" is not decoration. Two agents can open the
    /// queue at the same moment and both click Claim. Without it, the second
    /// click silently steals the trip from the first. With it, the second
    /// UPDATE changes no rows and this returns false, so the page can say "an
    /// agent has already taken this one".
    /// </summary>
    public bool ClaimByAgent(int itineraryId, int agentId)
        => _db.Execute(@"
        UPDATE  dbo.Itinerary
        SET     AgentId = @AgentId
        WHERE   ItineraryId = @ItineraryId
          AND   AgentId IS NULL;",
        Db.P("@ItineraryId", itineraryId),
        Db.P("@AgentId",     agentId)) > 0;

    public void SetStatus(int itineraryId, string status)
        => _db.Execute(@"
        UPDATE  dbo.Itinerary
        SET     Status = @Status
        WHERE   ItineraryId = @ItineraryId;",
        Db.P("@ItineraryId", itineraryId),
        Db.P("@Status",      status));

    /// <summary>
    /// Deletes a draft trip, or cancels it once anything has been booked on it.
    /// Returns true when it was cancelled rather than deleted.
    ///
    /// Deleting would work - the booking tables cascade - but it would erase
    /// the record of what the agency sold, and the revenue report would quietly
    /// lose money that was really earned.
    /// </summary>
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

    // -------------------------------------------------------------------------

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

    /// <summary>Map plus the two totals, for GetWithTotals.</summary>
    private static Itinerary MapWithTotals(SqlDataReader r)
    {
        var trip = Map(r);
        trip.BookingsTotal = (decimal)r["BookingsTotal"];
        trip.ExpensesTotal = (decimal)r["ExpensesTotal"];
        return trip;
    }
}
