using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class AccommodationRepository
{
    private readonly Db _db;

    public AccommodationRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      a.*,
                    p.ProvinceName,
                    (SELECT COUNT(*) FROM dbo.Room AS r
                     WHERE  r.AccommodationId = a.AccommodationId) AS RoomCount,

                    ISNULL((SELECT MIN(r2.PricePerNight) FROM dbo.Room AS r2
                            WHERE  r2.AccommodationId    = a.AccommodationId
                              AND  r2.AvailabilityStatus = 1), 0) AS FromPricePerNight,

                    ISNULL((SELECT SUM(r3.Capacity) FROM dbo.Room AS r3
                            WHERE  r3.AccommodationId    = a.AccommodationId
                              AND  r3.AvailabilityStatus = 1), 0) AS TotalCapacity
        FROM        dbo.Accommodation AS a
        LEFT JOIN   dbo.Province      AS p ON p.ProvinceId = a.ProvinceId";

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

    public List<Accommodation> Search(string? name = null, int? provinceId = null,
                                      byte? minStarRating = null, decimal? maxPrice = null,
                                      string? type = null, bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR a.IsActive = 1)
          AND   (@Name       IS NULL OR a.Name LIKE @Name ESCAPE '\')
          AND   (@ProvinceId IS NULL OR a.ProvinceId    = @ProvinceId)
          AND   (@MinStars   IS NULL OR a.StarRating   >= @MinStars)
          AND   (@MaxPrice   IS NULL OR EXISTS (
                    SELECT 1 FROM dbo.Room AS mr
                    WHERE  mr.AccommodationId    = a.AccommodationId
                      AND  mr.AvailabilityStatus = 1
                      AND  mr.PricePerNight     <= @MaxPrice))
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

    public int Create(Accommodation a)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Accommodation
              (Name,  Description,  Type,  Address,  City,  ProvinceId,
               StarRating,  PricePerNight,  Capacity,  Amenities)
        VALUES
              (@Name, @Description, @Type, @Address, @City, @ProvinceId,
               @StarRating, 1.00,           1,         @Amenities);

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
                Amenities     = @Amenities
        WHERE   AccommodationId = @AccommodationId;",
        parameters.ToArray());
    }

    public void SetImageUrl(int accommodationId, string? imageUrl)
        => _db.Execute(@"
        UPDATE  dbo.Accommodation
        SET     ImageUrl = @ImageUrl
        WHERE   AccommodationId = @AccommodationId;",
        Db.P("@AccommodationId", accommodationId),
        Db.P("@ImageUrl",        imageUrl));

    public bool Delete(int accommodationId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1
                              FROM        dbo.RoomBooking AS rb
                              INNER JOIN  dbo.Room        AS r ON r.RoomId = rb.RoomId
                              WHERE       r.AccommodationId = @AccommodationId)
                   OR EXISTS (SELECT 1 FROM dbo.TripLeg AS l
                              WHERE  l.AccommodationId = @AccommodationId)
                   OR EXISTS (SELECT 1 FROM dbo.TripRequestItem AS tr
                              WHERE  tr.AccommodationId = @AccommodationId)
                   OR EXISTS (SELECT 1
                              FROM        dbo.TripLegRoom AS lr
                              INNER JOIN  dbo.Room        AS r ON r.RoomId = lr.RoomId
                              WHERE       r.AccommodationId = @AccommodationId)
                 THEN 1 ELSE 0 END;

        IF @HasBookings = 1
        BEGIN
            UPDATE dbo.Accommodation
            SET    IsActive = 0
            WHERE  AccommodationId = @AccommodationId;

            UPDATE dbo.Room
            SET    AvailabilityStatus = 0
            WHERE  AccommodationId = @AccommodationId;
        END
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
        RoomCount       = Db.HasColumn(r, "RoomCount") ? (int)r["RoomCount"] : 0,
        FromPricePerNight = Db.HasColumn(r, "FromPricePerNight") ? (decimal)r["FromPricePerNight"] : 0m,
        TotalCapacity     = Db.HasColumn(r, "TotalCapacity") ? (int)r["TotalCapacity"] : 0,
        ImageUrl        = Db.GetOptionalString(r, "ImageUrl")
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
        Db.P("@Amenities",     a.Amenities)
    };
}

public class RoomRepository
{
    private readonly Db _db;

    public RoomRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      r.*,
                    a.Name AS AccommodationName
        FROM        dbo.Room          AS r
        INNER JOIN  dbo.Accommodation AS a ON a.AccommodationId = r.AccommodationId";

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

    public int Create(Room room)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Room (AccommodationId,  RoomNumber,  RoomType,  Beds,  Size,
                              Bathroom,  AvailabilityStatus,  PricePerNight,  Capacity)
        VALUES               (@AccommodationId, @RoomNumber, @RoomType, @Beds, @Size,
                              @Bathroom, @AvailabilityStatus, @PricePerNight, @Capacity);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@AccommodationId",    room.AccommodationId),
        Db.P("@RoomNumber",         room.RoomNumber.Trim()),
        Db.P("@RoomType",           room.RoomType),
        Db.P("@Beds",               room.Beds),
        Db.P("@Size",               room.Size),
        Db.P("@Bathroom",           room.Bathroom),
        Db.P("@AvailabilityStatus", room.AvailabilityStatus),
        Db.P("@PricePerNight",      room.PricePerNight),
        Db.P("@Capacity",           room.Capacity));

    public void Update(Room room)
        => _db.Execute(@"
        UPDATE  dbo.Room
        SET     RoomNumber         = @RoomNumber,
                RoomType           = @RoomType,
                Beds               = @Beds,
                Size               = @Size,
                Bathroom           = @Bathroom,
                AvailabilityStatus = @AvailabilityStatus,
                PricePerNight      = @PricePerNight,
                Capacity           = @Capacity
        WHERE   RoomId = @RoomId;",
        Db.P("@RoomId",             room.RoomId),
        Db.P("@RoomNumber",         room.RoomNumber.Trim()),
        Db.P("@RoomType",           room.RoomType),
        Db.P("@Beds",               room.Beds),
        Db.P("@Size",               room.Size),
        Db.P("@Bathroom",           room.Bathroom),
        Db.P("@AvailabilityStatus", room.AvailabilityStatus),
        Db.P("@PricePerNight",      room.PricePerNight),
        Db.P("@Capacity",           room.Capacity));

    public bool Delete(int roomId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1 FROM dbo.RoomBooking WHERE RoomId = @RoomId)
                   OR EXISTS (SELECT 1 FROM dbo.TripLegRoom WHERE RoomId = @RoomId)
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

    private static Room Map(SqlDataReader r) => new Room
    {
        RoomId             = (int)r["RoomId"],
        AccommodationId    = (int)r["AccommodationId"],
        AccommodationName  = Db.GetOptionalString(r, "AccommodationName"),
        RoomNumber         = (string)r["RoomNumber"],
        RoomType           = Db.GetNullableString(r, "RoomType"),
        Beds               = (int)r["Beds"],
        PricePerNight      = (decimal)r["PricePerNight"],
        Capacity           = (int)r["Capacity"],
        Size               = Db.GetNullableString(r, "Size"),
        Bathroom           = (bool)r["Bathroom"],
        AvailabilityStatus = (bool)r["AvailabilityStatus"]
    };
}
