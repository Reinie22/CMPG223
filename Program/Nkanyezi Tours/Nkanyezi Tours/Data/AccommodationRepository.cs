using Microsoft.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// Everything the system can do to accommodation and the rooms inside it.
///
/// Built by copying AttractionRepository. If you are writing your own, copy
/// that one - it has the fullest comments.
/// </summary>
public class AccommodationRepository
{
    private readonly Db _db;

    public AccommodationRepository(Db db)
    {
        _db = db;
    }

    /// <summary>
    /// The column list shared by every read.
    ///
    /// The correlated subquery counts the rooms without a GROUP BY over the
    /// whole table, which keeps the query readable. LEFT JOIN on Province
    /// because ProvinceId allows NULL and an INNER JOIN would hide every
    /// property whose province was never captured.
    /// </summary>
    private const string SelectSql = @"
        SELECT      a.*,
                    p.ProvinceName,
                    (SELECT COUNT(*) FROM dbo.Room AS r
                     WHERE  r.AccommodationId = a.AccommodationId) AS RoomCount
        FROM        dbo.Accommodation AS a
        LEFT JOIN   dbo.Province      AS p ON p.ProvinceId = a.ProvinceId";

    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

    public List<Accommodation> GetAll(bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR a.IsActive = 1)
        ORDER BY a.Name;",
        Map,
        Db.P("@IncludeInactive", includeInactive));

    public Accommodation? GetById(int accommodationId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   a.AccommodationId = @AccommodationId;",
        Map,
        Db.P("@AccommodationId", accommodationId));

    /// <summary>
    /// Filtered list for the search box. Every argument is optional - the
    /// "@Parameter IS NULL OR ..." pattern makes an unused filter drop away
    /// without building the WHERE clause out of string pieces.
    /// </summary>
    public List<Accommodation> Search(string? name = null, int? provinceId = null,
                                      byte? minStarRating = null, decimal? maxPrice = null,
                                      string? type = null, bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR a.IsActive = 1)
          AND   (@Name       IS NULL OR a.Name LIKE @Name ESCAPE '\')
          AND   (@ProvinceId IS NULL OR a.ProvinceId    = @ProvinceId)
          AND   (@MinStars   IS NULL OR a.StarRating   >= @MinStars)
          AND   (@MaxPrice   IS NULL OR a.PricePerNight <= @MaxPrice)
          AND   (@Type       IS NULL OR a.Type          = @Type)
        ORDER BY a.Name;",
        Map,
        Db.Like("@Name", name),
        Db.P("@ProvinceId",      provinceId),
        Db.P("@MinStars",        minStarRating),
        Db.P("@MaxPrice",        maxPrice),
        Db.P("@Type",            string.IsNullOrWhiteSpace(type) ? null : type),
        Db.P("@IncludeInactive", includeInactive));

    public bool NameExists(string name, int? excludeAccommodationId = null)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.Accommodation
        WHERE   Name = @Name
          AND   (@ExcludeId IS NULL OR AccommodationId <> @ExcludeId);",
        Db.P("@Name",      name.Trim()),
        Db.P("@ExcludeId", excludeAccommodationId)) > 0;

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

    public int Create(Accommodation a)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Accommodation
              (Name,  Description,  Type,  Address,  City,  ProvinceId,
               StarRating,  PricePerNight,  Capacity,  Amenities)
        VALUES
              (@Name, @Description, @Type, @Address, @City, @ProvinceId,
               @StarRating, @PricePerNight, @Capacity, @Amenities);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        BuildParameters(a));

    public void Update(Accommodation a)
    {
        var parameters = new List<SqlParameter> { Db.P("@AccommodationId", a.AccommodationId) };
        parameters.AddRange(BuildParameters(a));

        _db.Execute(@"
        UPDATE  dbo.Accommodation
        SET     Name          = @Name,
                Description   = @Description,
                Type          = @Type,
                Address       = @Address,
                City          = @City,
                ProvinceId    = @ProvinceId,
                StarRating    = @StarRating,
                PricePerNight = @PricePerNight,
                Capacity      = @Capacity,
                Amenities     = @Amenities
        WHERE   AccommodationId = @AccommodationId;",
        parameters.ToArray());
    }

    /// <summary>
    /// Removes the property, or deactivates it when any of its rooms have been
    /// booked. Returns true when it was deactivated rather than deleted.
    ///
    /// The EXISTS check reaches through Room into RoomBooking, because bookings
    /// point at a room and not at the property. Deleting the accommodation
    /// cascades to its rooms, which would take the bookings with them.
    /// </summary>
    public bool Delete(int accommodationId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1
                              FROM        dbo.RoomBooking AS rb
                              INNER JOIN  dbo.Room        AS r ON r.RoomId = rb.RoomId
                              WHERE       r.AccommodationId = @AccommodationId)
                 THEN 1 ELSE 0 END;

        IF @HasBookings = 1
            UPDATE dbo.Accommodation
            SET    IsActive = 0
            WHERE  AccommodationId = @AccommodationId;
        ELSE
            DELETE FROM dbo.Accommodation
            WHERE  AccommodationId = @AccommodationId;

        SELECT @HasBookings;",
        Db.P("@AccommodationId", accommodationId));

    public void Reactivate(int accommodationId)
        => _db.Execute(@"
        UPDATE  dbo.Accommodation
        SET     IsActive = 1
        WHERE   AccommodationId = @AccommodationId;",
        Db.P("@AccommodationId", accommodationId));

    // -------------------------------------------------------------------------

    private static Accommodation Map(SqlDataReader r) => new Accommodation
    {
        AccommodationId = (int)r["AccommodationId"],
        Name            = (string)r["Name"],
        Description     = Db.GetNullableString(r, "Description"),
        Type            = Db.GetNullableString(r, "Type"),
        Address         = Db.GetNullableString(r, "Address"),
        City            = Db.GetNullableString(r, "City"),
        ProvinceId      = Db.GetNullable<int>(r, "ProvinceId"),
        ProvinceName    = Db.GetOptionalString(r, "ProvinceName"),
        StarRating      = Db.GetNullable<byte>(r, "StarRating"),
        PricePerNight   = (decimal)r["PricePerNight"],
        Capacity        = (int)r["Capacity"],
        Amenities       = Db.GetNullableString(r, "Amenities"),
        IsActive        = (bool)r["IsActive"],
        CreatedDate     = (DateTime)r["CreatedDate"],
        RoomCount       = Db.HasColumn(r, "RoomCount") ? (int)r["RoomCount"] : 0
    };

    private static SqlParameter[] BuildParameters(Accommodation a) => new[]
    {
        Db.P("@Name",          a.Name.Trim()),
        Db.P("@Description",   a.Description),
        Db.P("@Type",          a.Type),
        Db.P("@Address",       a.Address),
        Db.P("@City",          a.City),
        Db.P("@ProvinceId",    a.ProvinceId),
        Db.P("@StarRating",    a.StarRating),
        Db.P("@PricePerNight", a.PricePerNight),
        Db.P("@Capacity",      a.Capacity),
        Db.P("@Amenities",     a.Amenities)
    };
}


/// <summary>
/// The rooms inside an accommodation, and the availability search that the
/// booking screen runs.
///
/// Separate from AccommodationRepository because the rooms screen is its own
/// page with its own owner. Both classes are registered in Program.cs.
/// </summary>
public class RoomRepository
{
    private readonly Db _db;

    public RoomRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      r.*,
                    a.Name          AS AccommodationName,
                    a.PricePerNight
        FROM        dbo.Room          AS r
        INNER JOIN  dbo.Accommodation AS a ON a.AccommodationId = r.AccommodationId";

    /// <summary>Every room in one property, in room-number order.</summary>
    public List<Room> GetByAccommodation(int accommodationId)
        => _db.Query(SelectSql + @"
        WHERE   r.AccommodationId = @AccommodationId
        ORDER BY r.RoomNumber;",
        Map,
        Db.P("@AccommodationId", accommodationId));

    public Room? GetById(int roomId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   r.RoomId = @RoomId;",
        Map,
        Db.P("@RoomId", roomId));

    /// <summary>
    /// The rooms in one property that are free for a date range. This is the
    /// query the "book accommodation" screen is built on.
    ///
    /// The clash test is the important part, and it is easy to get wrong. Two
    /// date ranges overlap when each one starts before the other one ends:
    ///
    ///     existing.CheckInDate  <  @CheckOutDate
    ///     existing.CheckOutDate >  @CheckInDate
    ///
    /// Strictly less-than and greater-than, not <= and >=, because a guest
    /// checking out on the 10th and another checking in on the 10th do not
    /// clash - the room is turned around that morning. Using <= here would
    /// wrongly block a third of your bookings, and it is the single most common
    /// bug in a booking system.
    ///
    /// A cancelled booking does not hold the room, which is the whole point of
    /// cancelling it.
    /// </summary>
    public List<Room> GetAvailable(int accommodationId, DateTime checkIn, DateTime checkOut)
        => _db.Query(SelectSql + @"
        WHERE   r.AccommodationId    = @AccommodationId
          AND   r.AvailabilityStatus = 1
          AND   NOT EXISTS (
                    SELECT  1
                    FROM    dbo.RoomBooking AS rb
                    WHERE   rb.RoomId  = r.RoomId
                      AND   rb.Status <> 'Cancelled'
                      AND   rb.CheckInDate  <  @CheckOutDate
                      AND   rb.CheckOutDate >  @CheckInDate)
        ORDER BY r.RoomNumber;",
        Map,
        Db.P("@AccommodationId", accommodationId),
        Db.P("@CheckInDate",     checkIn.Date),
        Db.P("@CheckOutDate",    checkOut.Date));

    /// <summary>
    /// True when this one room is free for the dates.
    ///
    /// Called again by the service just before saving, even though the user
    /// picked the room from an available list. Between loading that list and
    /// clicking Save, somebody else may have booked it. Checking once at the
    /// start is not enough.
    ///
    /// excludeRoomBookingId lets an existing booking be edited without clashing
    /// with itself.
    /// </summary>
    public bool IsAvailable(int roomId, DateTime checkIn, DateTime checkOut,
                            int? excludeRoomBookingId = null)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.RoomBooking
        WHERE   RoomId  = @RoomId
          AND   Status <> 'Cancelled'
          AND   CheckInDate  <  @CheckOutDate
          AND   CheckOutDate >  @CheckInDate
          AND   (@ExcludeId IS NULL OR RoomBookingId <> @ExcludeId);",
        Db.P("@RoomId",       roomId),
        Db.P("@CheckInDate",  checkIn.Date),
        Db.P("@CheckOutDate", checkOut.Date),
        Db.P("@ExcludeId",    excludeRoomBookingId)) == 0;

    // -------------------------------------------------------------------------

    public int Create(Room room)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Room (AccommodationId,  RoomNumber,  RoomType,  Beds,  Size,
                              Bathroom,  AvailabilityStatus)
        VALUES               (@AccommodationId, @RoomNumber, @RoomType, @Beds, @Size,
                              @Bathroom, @AvailabilityStatus);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@AccommodationId",    room.AccommodationId),
        Db.P("@RoomNumber",         room.RoomNumber.Trim()),
        Db.P("@RoomType",           room.RoomType),
        Db.P("@Beds",               room.Beds),
        Db.P("@Size",               room.Size),
        Db.P("@Bathroom",           room.Bathroom),
        Db.P("@AvailabilityStatus", room.AvailabilityStatus));

    public void Update(Room room)
        => _db.Execute(@"
        UPDATE  dbo.Room
        SET     RoomNumber         = @RoomNumber,
                RoomType           = @RoomType,
                Beds               = @Beds,
                Size               = @Size,
                Bathroom           = @Bathroom,
                AvailabilityStatus = @AvailabilityStatus
        WHERE   RoomId = @RoomId;",
        Db.P("@RoomId",             room.RoomId),
        Db.P("@RoomNumber",         room.RoomNumber.Trim()),
        Db.P("@RoomType",           room.RoomType),
        Db.P("@Beds",               room.Beds),
        Db.P("@Size",               room.Size),
        Db.P("@Bathroom",           room.Bathroom),
        Db.P("@AvailabilityStatus", room.AvailabilityStatus));

    /// <summary>
    /// Deletes a room, or takes it out of service when it has been booked.
    /// Returns true when it was taken out of service rather than deleted.
    /// </summary>
    public bool Delete(int roomId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1 FROM dbo.RoomBooking WHERE RoomId = @RoomId)
                 THEN 1 ELSE 0 END;

        IF @HasBookings = 1
            UPDATE dbo.Room
            SET    AvailabilityStatus = 0
            WHERE  RoomId = @RoomId;
        ELSE
            DELETE FROM dbo.Room
            WHERE  RoomId = @RoomId;

        SELECT @HasBookings;",
        Db.P("@RoomId", roomId));

    // -------------------------------------------------------------------------

    private static Room Map(SqlDataReader r) => new Room
    {
        RoomId             = (int)r["RoomId"],
        AccommodationId    = (int)r["AccommodationId"],
        AccommodationName  = Db.GetOptionalString(r, "AccommodationName"),
        RoomNumber         = (string)r["RoomNumber"],
        RoomType           = Db.GetNullableString(r, "RoomType"),
        Beds               = (int)r["Beds"],
        Size               = Db.GetNullableString(r, "Size"),
        Bathroom           = (bool)r["Bathroom"],
        AvailabilityStatus = (bool)r["AvailabilityStatus"],
        PricePerNight      = Db.HasColumn(r, "PricePerNight") ? (decimal)r["PricePerNight"] : 0m
    };
}
