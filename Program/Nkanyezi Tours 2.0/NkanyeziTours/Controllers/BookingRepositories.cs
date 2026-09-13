using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class RoomBookingRepository
{
    private readonly Db _db;

    public RoomBookingRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      rb.*,
                    r.RoomNumber,
                    r.RoomType,
                    acc.AccommodationId,
                    acc.Name AS AccommodationName,
                    acc.City,
                    CASE WHEN u.UserId IS NULL THEN NULL
                         ELSE u.Name + ' ' + u.Surname END AS CreatedByName
        FROM        dbo.RoomBooking   AS rb
        INNER JOIN  dbo.Room          AS r   ON r.RoomId = rb.RoomId
        INNER JOIN  dbo.Accommodation AS acc ON acc.AccommodationId = r.AccommodationId
        LEFT JOIN   dbo.AppUser       AS u   ON u.UserId = rb.CreatedBy";

    public List<RoomBooking> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE   rb.ItineraryId = @ItineraryId
        ORDER BY rb.CheckInDate;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public RoomBooking? GetById(int roomBookingId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   rb.RoomBookingId = @RoomBookingId;",
        Map,
        Db.P("@RoomBookingId", roomBookingId));

    public int Create(RoomBooking booking)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.RoomBooking (RoomId, ItineraryId, CheckInDate, CheckOutDate,
                                     TotalCost, Status, CreatedBy)
        SELECT      @RoomId, @ItineraryId, @CheckInDate, @CheckOutDate,
                    @TotalCost, @Status, @CreatedBy
        WHERE       NOT EXISTS (
                        SELECT  1
                        FROM    dbo.RoomBooking
                        WHERE   RoomId  = @RoomId
                          AND   Status <> 'Cancelled'
                          AND   CheckInDate  <  @CheckOutDate
                          AND   CheckOutDate >  @CheckInDate);

        SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS INT);",
        Db.P("@RoomId",       booking.RoomId),
        Db.P("@ItineraryId",  booking.ItineraryId),
        Db.P("@CheckInDate",  booking.CheckInDate.Date),
        Db.P("@CheckOutDate", booking.CheckOutDate.Date),
        Db.P("@TotalCost",    booking.TotalCost),
        Db.P("@Status",       booking.Status),
        Db.P("@CreatedBy",    booking.CreatedBy));

    public void Update(RoomBooking booking)
        => _db.Execute(@"
        UPDATE  dbo.RoomBooking
        SET     CheckInDate  = @CheckInDate,
                CheckOutDate = @CheckOutDate,
                TotalCost    = @TotalCost,
                Status       = @Status
        WHERE   RoomBookingId = @RoomBookingId;",
        Db.P("@RoomBookingId", booking.RoomBookingId),
        Db.P("@CheckInDate",   booking.CheckInDate.Date),
        Db.P("@CheckOutDate",  booking.CheckOutDate.Date),
        Db.P("@TotalCost",     booking.TotalCost),
        Db.P("@Status",        booking.Status));

    public void Cancel(int roomBookingId)
        => _db.Execute(@"
        UPDATE  dbo.RoomBooking
        SET     Status = 'Cancelled'
        WHERE   RoomBookingId = @RoomBookingId;",
        Db.P("@RoomBookingId", roomBookingId));

    public void Delete(int roomBookingId)
        => _db.Execute(@"
        DELETE FROM dbo.RoomBooking WHERE RoomBookingId = @RoomBookingId;",
        Db.P("@RoomBookingId", roomBookingId));

    private static RoomBooking Map(SqlDataReader r) => new RoomBooking
    {
        RoomBookingId     = (int)r["RoomBookingId"],
        RoomId            = (int)r["RoomId"],
        ItineraryId       = (int)r["ItineraryId"],
        RoomNumber        = Db.GetOptionalString(r, "RoomNumber"),
        RoomType          = Db.GetOptionalString(r, "RoomType"),
        AccommodationId   = Db.HasColumn(r, "AccommodationId")
                                ? Db.GetNullable<int>(r, "AccommodationId") : null,
        AccommodationName = Db.GetOptionalString(r, "AccommodationName"),
        City              = Db.GetOptionalString(r, "City"),
        CheckInDate       = (DateTime)r["CheckInDate"],
        CheckOutDate      = (DateTime)r["CheckOutDate"],
        TotalCost         = (decimal)r["TotalCost"],
        Status            = (string)r["Status"],
        CreatedDate       = (DateTime)r["CreatedDate"],
        CreatedBy         = Db.GetNullable<int>(r, "CreatedBy"),
        CreatedByName     = Db.GetOptionalString(r, "CreatedByName")
    };
}

public class AttractionBookingRepository
{
    private readonly Db _db;

    public AttractionBookingRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      ab.*,
                    att.Name AS AttractionName,
                    att.City,
                    CASE WHEN u.UserId IS NULL THEN NULL
                         ELSE u.Name + ' ' + u.Surname END AS CreatedByName
        FROM        dbo.AttractionBooking AS ab
        INNER JOIN  dbo.Attraction        AS att ON att.AttractionId = ab.AttractionId
        LEFT JOIN   dbo.AppUser           AS u   ON u.UserId         = ab.CreatedBy";

    public List<AttractionBooking> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE   ab.ItineraryId = @ItineraryId
        ORDER BY ab.BookingDate;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public AttractionBooking? GetById(int attractionBookingId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   ab.AttractionBookingId = @AttractionBookingId;",
        Map,
        Db.P("@AttractionBookingId", attractionBookingId));

    public int Create(AttractionBooking booking)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.AttractionBooking (AttractionId, ItineraryId,
                                           BookingDate, Quantity, TotalCost,
                                           Status, CreatedBy)
        SELECT      @AttractionId, @ItineraryId,
                    @BookingDate, @Quantity, @TotalCost,
                    @Status, @CreatedBy
        FROM        dbo.Attraction AS a
        WHERE       a.AttractionId = @AttractionId
          AND       @Quantity + ISNULL((
                        SELECT  SUM(ab.Quantity)
                        FROM    dbo.AttractionBooking AS ab
                        WHERE   ab.AttractionId = @AttractionId
                          AND   ab.BookingDate  = @BookingDate
                          AND   ab.Status     <> 'Cancelled'), 0) <= a.Quantity;

        SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS INT);",
        Db.P("@AttractionId", booking.AttractionId),
        Db.P("@ItineraryId",  booking.ItineraryId),
        Db.P("@BookingDate",  booking.BookingDate.Date),
        Db.P("@Quantity",     booking.Quantity),
        Db.P("@TotalCost",    booking.TotalCost),
        Db.P("@Status",       booking.Status),
        Db.P("@CreatedBy",    booking.CreatedBy));

    public void Update(AttractionBooking booking)
        => _db.Execute(@"
        UPDATE  dbo.AttractionBooking
        SET     BookingDate = @BookingDate,
                Quantity    = @Quantity,
                TotalCost   = @TotalCost,
                Status      = @Status
        WHERE   AttractionBookingId = @AttractionBookingId;",
        Db.P("@AttractionBookingId", booking.AttractionBookingId),
        Db.P("@BookingDate",         booking.BookingDate.Date),
        Db.P("@Quantity",            booking.Quantity),
        Db.P("@TotalCost",           booking.TotalCost),
        Db.P("@Status",              booking.Status));

    public void Cancel(int attractionBookingId)
        => _db.Execute(@"
        UPDATE  dbo.AttractionBooking
        SET     Status = 'Cancelled'
        WHERE   AttractionBookingId = @AttractionBookingId;",
        Db.P("@AttractionBookingId", attractionBookingId));

    public void Delete(int attractionBookingId)
        => _db.Execute(@"
        DELETE FROM dbo.AttractionBooking WHERE AttractionBookingId = @AttractionBookingId;",
        Db.P("@AttractionBookingId", attractionBookingId));

    private static AttractionBooking Map(SqlDataReader r) => new AttractionBooking
    {
        AttractionBookingId = (int)r["AttractionBookingId"],
        AttractionId        = (int)r["AttractionId"],
        ItineraryId         = (int)r["ItineraryId"],
        AttractionName      = Db.GetOptionalString(r, "AttractionName"),
        City                = Db.GetOptionalString(r, "City"),
        BookingDate         = (DateTime)r["BookingDate"],
        Quantity            = (int)r["Quantity"],
        TotalCost           = (decimal)r["TotalCost"],
        Status              = (string)r["Status"],
        CreatedDate         = (DateTime)r["CreatedDate"],
        CreatedBy           = Db.GetNullable<int>(r, "CreatedBy"),
        CreatedByName       = Db.GetOptionalString(r, "CreatedByName")
    };
}

public class VehicleBookingRepository
{
    private readonly Db _db;

    public VehicleBookingRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      vb.*,
                    v.Make,
                    v.Model,
                    v.RegistrationNumber,
                    v.Location,
                    CASE WHEN u.UserId IS NULL THEN NULL
                         ELSE u.Name + ' ' + u.Surname END AS CreatedByName
        FROM        dbo.VehicleBooking AS vb
        INNER JOIN  dbo.Vehicle        AS v ON v.VehicleId = vb.VehicleId
        LEFT JOIN   dbo.AppUser        AS u ON u.UserId    = vb.CreatedBy";

    public List<VehicleBooking> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE   vb.ItineraryId = @ItineraryId
        ORDER BY vb.StartDate;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public VehicleBooking? GetById(int vehicleBookingId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   vb.VehicleBookingId = @VehicleBookingId;",
        Map,
        Db.P("@VehicleBookingId", vehicleBookingId));

    public List<VehicleBooking> GetOutstandingReturns()
        => _db.Query(SelectSql + @"
        WHERE   vb.Status = 'Collected'
        ORDER BY vb.EndDate;",
        Map);

    public int Create(VehicleBooking booking)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.VehicleBooking (VehicleId, ItineraryId, StartDate, EndDate,
                                        RatePerDay, IncludedKmPerDay, RatePerExtraKm,
                                        Status, CreatedBy)
        SELECT      v.VehicleId, @ItineraryId, @StartDate, @EndDate,
                    v.PricePerDay, v.IncludedKmPerDay, v.RatePerExtraKm,
                    @Status, @CreatedBy
        FROM        dbo.Vehicle AS v
        WHERE       v.VehicleId = @VehicleId
          AND       NOT EXISTS (
                        SELECT  1
                        FROM    dbo.VehicleBooking AS vb
                        WHERE   vb.VehicleId = @VehicleId
                          AND   vb.Status   <> 'Cancelled'
                          AND   vb.StartDate <= @EndDate
                          AND   vb.EndDate   >= @StartDate);

        SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS INT);",
        Db.P("@VehicleId",   booking.VehicleId),
        Db.P("@ItineraryId", booking.ItineraryId),
        Db.P("@StartDate",   booking.StartDate.Date),
        Db.P("@EndDate",     booking.EndDate.Date),
        Db.P("@Status",      booking.Status),
        Db.P("@CreatedBy",   booking.CreatedBy));

    public void UpdateDates(int vehicleBookingId, DateTime startDate, DateTime endDate)
        => _db.Execute(@"
        UPDATE  dbo.VehicleBooking
        SET     StartDate = @StartDate,
                EndDate   = @EndDate
        WHERE   VehicleBookingId = @VehicleBookingId;",
        Db.P("@VehicleBookingId", vehicleBookingId),
        Db.P("@StartDate",        startDate.Date),
        Db.P("@EndDate",          endDate.Date));

    public bool Collect(int vehicleBookingId, int odometerOut)
        => _db.Execute(@"
        UPDATE  dbo.VehicleBooking
        SET     OdometerOut = @OdometerOut,
                Status      = 'Collected'
        WHERE   VehicleBookingId = @VehicleBookingId
          AND   Status = 'Confirmed';",
        Db.P("@VehicleBookingId", vehicleBookingId),
        Db.P("@OdometerOut",      odometerOut)) > 0;

    public bool Return(int vehicleBookingId, int odometerIn)
        => _db.Execute(@"
        UPDATE  dbo.VehicleBooking
        SET     OdometerIn = @OdometerIn,
                Status     = 'Returned'
        WHERE   VehicleBookingId = @VehicleBookingId
          AND   Status      = 'Collected'
          AND   OdometerOut IS NOT NULL
          AND   @OdometerIn >= OdometerOut;",
        Db.P("@VehicleBookingId", vehicleBookingId),
        Db.P("@OdometerIn",       odometerIn)) > 0;

    public void Cancel(int vehicleBookingId)
        => _db.Execute(@"
        UPDATE  dbo.VehicleBooking
        SET     Status = 'Cancelled'
        WHERE   VehicleBookingId = @VehicleBookingId;",
        Db.P("@VehicleBookingId", vehicleBookingId));

    public void Delete(int vehicleBookingId)
        => _db.Execute(@"
        DELETE FROM dbo.VehicleBooking WHERE VehicleBookingId = @VehicleBookingId;",
        Db.P("@VehicleBookingId", vehicleBookingId));

    private static VehicleBooking Map(SqlDataReader r) => new VehicleBooking
    {
        VehicleBookingId   = (int)r["VehicleBookingId"],
        VehicleId          = (int)r["VehicleId"],
        ItineraryId        = (int)r["ItineraryId"],
        Make               = Db.GetOptionalString(r, "Make"),
        Model              = Db.GetOptionalString(r, "Model"),
        RegistrationNumber = Db.GetOptionalString(r, "RegistrationNumber"),
        Location           = Db.GetOptionalString(r, "Location"),
        StartDate          = (DateTime)r["StartDate"],
        EndDate            = (DateTime)r["EndDate"],
        RatePerDay         = (decimal)r["RatePerDay"],
        IncludedKmPerDay   = (int)r["IncludedKmPerDay"],
        RatePerExtraKm     = (decimal)r["RatePerExtraKm"],
        OdometerOut        = Db.GetNullable<int>(r, "OdometerOut"),
        OdometerIn         = Db.GetNullable<int>(r, "OdometerIn"),

        TotalDays          = (int)r["TotalDays"],
        KmDriven           = Db.GetNullable<int>(r, "KmDriven"),
        KmIncluded         = (int)r["KmIncluded"],
        ExtraKm            = (int)r["ExtraKm"],
        BaseCost           = (decimal)r["BaseCost"],
        ExtraKmCost        = (decimal)r["ExtraKmCost"],
        TotalCost          = (decimal)r["TotalCost"],

        Status             = (string)r["Status"],
        CreatedDate        = (DateTime)r["CreatedDate"],
        CreatedBy          = Db.GetNullable<int>(r, "CreatedBy"),
        CreatedByName      = Db.GetOptionalString(r, "CreatedByName")
    };
}

public class RouteBookingRepository
{
    private readonly Db _db;

    public RouteBookingRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      rtb.*,
                    rt.RouteType,
                    rt.Carrier,
                    rt.RouteCode,
                    rt.DepartureCity,
                    rt.ArrivalCity,
                    rt.DepartureTime,
                    CASE WHEN u.UserId IS NULL THEN NULL
                         ELSE u.Name + ' ' + u.Surname END AS CreatedByName
        FROM        dbo.RouteBooking AS rtb
        INNER JOIN  dbo.Route        AS rt ON rt.RouteId = rtb.RouteId
        LEFT JOIN   dbo.AppUser      AS u  ON u.UserId   = rtb.CreatedBy";

    public List<RouteBooking> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE   rtb.ItineraryId = @ItineraryId
        ORDER BY rtb.TravelDate, rt.DepartureTime;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public RouteBooking? GetById(int routeBookingId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   rtb.RouteBookingId = @RouteBookingId;",
        Map,
        Db.P("@RouteBookingId", routeBookingId));

    public int Create(RouteBooking booking)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.RouteBooking (RouteId, ItineraryId, TravelDate, Seats,
                                      FarePerSeat, BookingReference, Status, CreatedBy)
        SELECT @RouteId, @ItineraryId, @TravelDate, @Seats,
               @FarePerSeat, @BookingReference, @Status, @CreatedBy
        FROM   dbo.Route AS r WITH (UPDLOCK, HOLDLOCK)
        WHERE  r.RouteId = @RouteId
          AND  r.IsActive = 1
          AND  @Seats + ISNULL((
                   SELECT SUM(existing.Seats)
                   FROM dbo.RouteBooking AS existing
                   WHERE existing.RouteId = @RouteId
                     AND existing.TravelDate = @TravelDate
                     AND existing.Status <> 'Cancelled'), 0) <= r.SeatCapacity;

        SELECT CAST(ISNULL(SCOPE_IDENTITY(), 0) AS INT);",
        Db.P("@RouteId",          booking.RouteId),
        Db.P("@ItineraryId",      booking.ItineraryId),
        Db.P("@TravelDate",       booking.TravelDate.Date),
        Db.P("@Seats",            booking.Seats),
        Db.P("@FarePerSeat",      booking.FarePerSeat),
        Db.P("@BookingReference", booking.BookingReference),
        Db.P("@Status",           booking.Status),
        Db.P("@CreatedBy",        booking.CreatedBy));

    public void Update(RouteBooking booking)
        => _db.Execute(@"
        UPDATE  dbo.RouteBooking
        SET     TravelDate       = @TravelDate,
                Seats            = @Seats,
                FarePerSeat      = @FarePerSeat,
                BookingReference = @BookingReference,
                Status           = @Status
        WHERE   RouteBookingId = @RouteBookingId;",
        Db.P("@RouteBookingId",   booking.RouteBookingId),
        Db.P("@TravelDate",       booking.TravelDate.Date),
        Db.P("@Seats",            booking.Seats),
        Db.P("@FarePerSeat",      booking.FarePerSeat),
        Db.P("@BookingReference", booking.BookingReference),
        Db.P("@Status",           booking.Status));

    public void Cancel(int routeBookingId)
        => _db.Execute(@"
        UPDATE  dbo.RouteBooking
        SET     Status = 'Cancelled'
        WHERE   RouteBookingId = @RouteBookingId;",
        Db.P("@RouteBookingId", routeBookingId));

    public void Delete(int routeBookingId)
        => _db.Execute(@"
        DELETE FROM dbo.RouteBooking WHERE RouteBookingId = @RouteBookingId;",
        Db.P("@RouteBookingId", routeBookingId));

    private static RouteBooking Map(SqlDataReader r) => new RouteBooking
    {
        RouteBookingId   = (int)r["RouteBookingId"],
        RouteId          = (int)r["RouteId"],
        ItineraryId      = (int)r["ItineraryId"],
        RouteType        = Db.GetOptionalString(r, "RouteType"),
        Carrier          = Db.GetOptionalString(r, "Carrier"),
        RouteCode        = Db.GetOptionalString(r, "RouteCode"),
        DepartureCity    = Db.GetOptionalString(r, "DepartureCity"),
        ArrivalCity      = Db.GetOptionalString(r, "ArrivalCity"),
        DepartureTime    = Db.HasColumn(r, "DepartureTime")
                               ? Db.GetNullable<TimeSpan>(r, "DepartureTime") : null,
        TravelDate       = (DateTime)r["TravelDate"],
        Seats            = (int)r["Seats"],
        FarePerSeat      = (decimal)r["FarePerSeat"],
        TotalCost        = (decimal)r["TotalCost"],
        BookingReference = Db.GetNullableString(r, "BookingReference"),
        Status           = (string)r["Status"],
        CreatedDate      = (DateTime)r["CreatedDate"],
        CreatedBy        = Db.GetNullable<int>(r, "CreatedBy"),
        CreatedByName    = Db.GetOptionalString(r, "CreatedByName")
    };
}
