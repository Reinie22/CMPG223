using Microsoft.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// Everything the system can do to attractions.
///
/// ============================================================================
/// THIS IS THE PATTERN. Every other repository is this file with different
/// table and column names. Copy it.
/// ============================================================================
///
/// The rules:
///
///   1. The SQL is written here, as text, and it is the only place SQL appears.
///      Never in a page, never in a service.
///
///   2. EVERY value is a parameter. Look at how the SQL below is built: the
///      strings are constants, and nothing the user typed is ever glued into
///      them. See the long comment in Db.cs for why this matters - it is the
///      difference between a working system and one that anybody can empty.
///
///   3. One private Map method turns a database row into a model object, and
///      every read method uses it. Write the mapping once.
///
///   4. Methods return models or lists of models. Never a DataTable, never a
///      SqlDataReader. Nothing outside this folder should know we use SQL
///      Server at all.
///
/// How a page uses it:
///     var list = _attractions.GetAll();
/// </summary>
public class AttractionRepository
{
    private readonly Db _db;

    public AttractionRepository(Db db)
    {
        _db = db;
    }

    /// <summary>
    /// The column list shared by every read, written once.
    ///
    /// The joins bring in the category and province names so the list screen
    /// can show "Wildlife" and "Gauteng" instead of 3 and 7. LEFT JOIN and not
    /// INNER JOIN, because both columns allow NULL and an INNER JOIN would
    /// silently hide every attraction whose category was never captured. That
    /// mistake is very easy to make and very hard to spot: the page just shows
    /// fewer rows than it should.
    /// </summary>
    private const string SelectSql = @"
        SELECT  a.*,
                c.CategoryName,
                p.ProvinceName
        FROM        dbo.Attraction         AS a
        LEFT JOIN   dbo.AttractionCategory AS c ON c.CategoryId  = a.CategoryId
        LEFT JOIN   dbo.Province           AS p ON p.ProvinceId  = a.ProvinceId";

    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

    /// <summary>Every active attraction, sorted by name. For the list screen.</summary>
    public List<Attraction> GetAll(bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR a.IsActive = 1)
        ORDER BY a.Name;",
        Map,
        Db.P("@IncludeInactive", includeInactive));

    /// <summary>One attraction, or null when the id does not exist.</summary>
    public Attraction? GetById(int attractionId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   a.AttractionId = @AttractionId;",
        Map,
        Db.P("@AttractionId", attractionId));

    /// <summary>
    /// Filtered list for the search box. Every argument is optional: leave one
    /// out and that filter is ignored.
    ///
    /// The trick is the "@Parameter IS NULL OR ..." pattern on each line. When
    /// the user leaves the category dropdown empty we pass NULL, the left half
    /// of the OR is true, and that whole condition drops away. One query serves
    /// every combination of filters, and - critically - it does so WITHOUT
    /// building the WHERE clause out of string pieces. Concatenating a WHERE
    /// clause is how search boxes become the injection hole in most systems.
    ///
    /// ESCAPE '\' goes with Db.Like(), which wraps the text in % signs and
    /// escapes any % or _ the user typed, so searching for "50%" does not match
    /// everything.
    /// </summary>
    public List<Attraction> Search(string? name = null, int? categoryId = null,
                                   int? provinceId = null, decimal? maxRate = null,
                                   bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR a.IsActive = 1)
          AND   (@Name       IS NULL OR a.Name LIKE @Name ESCAPE '\')
          AND   (@CategoryId IS NULL OR a.CategoryId = @CategoryId)
          AND   (@ProvinceId IS NULL OR a.ProvinceId = @ProvinceId)
          AND   (@MaxRate    IS NULL OR a.Rate      <= @MaxRate)
        ORDER BY a.Name;",
        Map,
        Db.Like("@Name", name),
        Db.P("@CategoryId",      categoryId),
        Db.P("@ProvinceId",      provinceId),
        Db.P("@MaxRate",         maxRate),
        Db.P("@IncludeInactive", includeInactive));

    /// <summary>True when another attraction already uses this name.</summary>
    public bool NameExists(string name, int? excludeAttractionId = null)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.Attraction
        WHERE   Name = @Name
          AND   (@ExcludeId IS NULL OR AttractionId <> @ExcludeId);",
        Db.P("@Name",      name.Trim()),
        Db.P("@ExcludeId", excludeAttractionId)) > 0;

    // -------------------------------------------------------------------------
    // Time slots
    // -------------------------------------------------------------------------

    /// <summary>The slots belonging to one attraction, earliest first.</summary>
    public List<TimeSlot> GetTimeSlots(int attractionId)
        => _db.Query(@"
        SELECT  *
        FROM    dbo.TimeSlot
        WHERE   AttractionId = @AttractionId
        ORDER BY TimeStart;",
        MapTimeSlot,
        Db.P("@AttractionId", attractionId));

    public int AddTimeSlot(TimeSlot slot)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.TimeSlot (AttractionId, TimeStart, TimeEnd)
        VALUES                   (@AttractionId, @TimeStart, @TimeEnd);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        Db.P("@AttractionId", slot.AttractionId),
        Db.P("@TimeStart",    slot.TimeStart),
        Db.P("@TimeEnd",      slot.TimeEnd));

    /// <summary>
    /// Removes a slot. Refuses when somebody has already booked it, because
    /// deleting it would take their booking with it.
    /// Returns false when the slot was left alone for that reason.
    /// </summary>
    public bool DeleteTimeSlot(int timeSlotId)
        => _db.Execute(@"
        DELETE FROM dbo.TimeSlot
        WHERE   TimeSlotId = @TimeSlotId
          AND   NOT EXISTS (SELECT 1
                            FROM   dbo.AttractionBooking
                            WHERE  TimeSlotId = @TimeSlotId);",
        Db.P("@TimeSlotId", timeSlotId)) > 0;

    /// <summary>
    /// The slots for one attraction on one date, showing how many spaces are
    /// left in each. The booking screen fills its dropdown from this.
    ///
    /// Quantity on the attraction is the capacity of a single slot, so the sum
    /// of everyone already booked into that slot on that date is subtracted
    /// from it. Cancelled bookings do not count against capacity, which is the
    /// whole point of cancelling one.
    ///
    /// LEFT JOIN again: a slot nobody has booked yet must still appear, showing
    /// its full capacity. An INNER JOIN would hide exactly the slots that are
    /// most available.
    /// </summary>
    public List<TimeSlotAvailability> GetAvailability(int attractionId, DateTime bookingDate)
        => _db.Query(@"
        SELECT      ts.TimeSlotId,
                    ts.TimeStart,
                    ts.TimeEnd,
                    a.Quantity                              AS Capacity,
                    ISNULL(SUM(ab.Quantity), 0)             AS AlreadyBooked,
                    a.Quantity - ISNULL(SUM(ab.Quantity), 0) AS SpacesLeft
        FROM        dbo.TimeSlot          AS ts
        INNER JOIN  dbo.Attraction        AS a  ON a.AttractionId = ts.AttractionId
        LEFT JOIN   dbo.AttractionBooking AS ab ON ab.TimeSlotId  = ts.TimeSlotId
                                              AND ab.BookingDate  = @BookingDate
                                              AND ab.Status      <> 'Cancelled'
        WHERE       ts.AttractionId = @AttractionId
        GROUP BY    ts.TimeSlotId, ts.TimeStart, ts.TimeEnd, a.Quantity
        ORDER BY    ts.TimeStart;",
        r => new TimeSlotAvailability
        {
            TimeSlotId    = (int)r["TimeSlotId"],
            TimeStart     = (TimeSpan)r["TimeStart"],
            TimeEnd       = (TimeSpan)r["TimeEnd"],
            Capacity      = (int)r["Capacity"],
            AlreadyBooked = (int)r["AlreadyBooked"],
            SpacesLeft    = (int)r["SpacesLeft"]
        },
        Db.P("@AttractionId", attractionId),
        Db.P("@BookingDate",  bookingDate.Date));

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a new attraction and returns its new id.
    ///
    /// SCOPE_IDENTITY gives back the id SQL Server generated for this row. Use
    /// it rather than @@IDENTITY, which can return an id created by a trigger
    /// somewhere else entirely.
    /// </summary>
    public int Create(Attraction a)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.Attraction
              (Name,  CategoryId,  Address,  City,  ProvinceId,  Description,
               Rate,  Quantity,    UnitOfMeasure,   DurationMinutes,
               OperatingTimeStart, OperatingTimeEnd, BestOperatingSeason)
        VALUES
              (@Name, @CategoryId, @Address, @City, @ProvinceId, @Description,
               @Rate, @Quantity,   @UnitOfMeasure,  @DurationMinutes,
               @OperatingTimeStart, @OperatingTimeEnd, @BestOperatingSeason);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        BuildParameters(a));

    /// <summary>
    /// Saves changes to an existing attraction.
    ///
    /// Note the WHERE clause. An UPDATE without one rewrites every row in the
    /// table, and there is no undo. Check for it every single time you write
    /// an UPDATE or a DELETE.
    /// </summary>
    public void Update(Attraction a)
    {
        var parameters = new List<SqlParameter> { Db.P("@AttractionId", a.AttractionId) };
        parameters.AddRange(BuildParameters(a));

        _db.Execute(@"
        UPDATE  dbo.Attraction
        SET     Name                = @Name,
                CategoryId          = @CategoryId,
                Address             = @Address,
                City                = @City,
                ProvinceId          = @ProvinceId,
                Description         = @Description,
                Rate                = @Rate,
                Quantity            = @Quantity,
                UnitOfMeasure       = @UnitOfMeasure,
                DurationMinutes     = @DurationMinutes,
                OperatingTimeStart  = @OperatingTimeStart,
                OperatingTimeEnd    = @OperatingTimeEnd,
                BestOperatingSeason = @BestOperatingSeason
        WHERE   AttractionId = @AttractionId;",
        parameters.ToArray());
    }

    /// <summary>
    /// Removes an attraction, or deactivates it when it has been booked before.
    ///
    /// Returns true when the record was deactivated rather than deleted, so the
    /// page can tell the user what actually happened instead of claiming it was
    /// deleted while it is still sitting there.
    ///
    /// Both branches are one statement sent to the database together. Doing the
    /// check in C# first and then deleting would leave a gap where somebody
    /// else books the attraction between the two calls, and the delete would
    /// fail on the foreign key.
    /// </summary>
    public bool Delete(int attractionId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1
                              FROM   dbo.AttractionBooking
                              WHERE  AttractionId = @AttractionId)
                 THEN 1 ELSE 0 END;

        IF @HasBookings = 1
            UPDATE dbo.Attraction
            SET    IsActive = 0
            WHERE  AttractionId = @AttractionId;
        ELSE
            DELETE FROM dbo.Attraction
            WHERE  AttractionId = @AttractionId;

        SELECT @HasBookings;",
        Db.P("@AttractionId", attractionId));

    /// <summary>Puts a deactivated attraction back into service.</summary>
    public void Reactivate(int attractionId)
        => _db.Execute(@"
        UPDATE  dbo.Attraction
        SET     IsActive = 1
        WHERE   AttractionId = @AttractionId;",
        Db.P("@AttractionId", attractionId));

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Turns one database row into an Attraction.
    /// Written once here, used by every read method above. When a column is
    /// added to the table, this is the only line to change.
    /// </summary>
    private static Attraction Map(SqlDataReader r) => new Attraction
    {
        AttractionId        = (int)r["AttractionId"],
        Name                = (string)r["Name"],
        CategoryId          = Db.GetNullable<int>(r, "CategoryId"),
        CategoryName        = Db.GetOptionalString(r, "CategoryName"),
        Address             = Db.GetNullableString(r, "Address"),
        City                = Db.GetNullableString(r, "City"),
        ProvinceId          = Db.GetNullable<int>(r, "ProvinceId"),
        ProvinceName        = Db.GetOptionalString(r, "ProvinceName"),
        Description         = Db.GetNullableString(r, "Description"),
        Rate                = (decimal)r["Rate"],
        Quantity            = (int)r["Quantity"],
        UnitOfMeasure       = Db.GetNullableString(r, "UnitOfMeasure"),
        DurationMinutes     = Db.GetNullable<int>(r, "DurationMinutes"),
        OperatingTimeStart  = Db.GetNullable<TimeSpan>(r, "OperatingTimeStart"),
        OperatingTimeEnd    = Db.GetNullable<TimeSpan>(r, "OperatingTimeEnd"),
        BestOperatingSeason = Db.GetNullableString(r, "BestOperatingSeason"),
        IsActive            = (bool)r["IsActive"],
        CreatedDate         = (DateTime)r["CreatedDate"]
    };

    private static TimeSlot MapTimeSlot(SqlDataReader r) => new TimeSlot
    {
        TimeSlotId   = (int)r["TimeSlotId"],
        AttractionId = (int)r["AttractionId"],
        TimeStart    = (TimeSpan)r["TimeStart"],
        TimeEnd      = (TimeSpan)r["TimeEnd"]
    };

    /// <summary>
    /// The parameters shared by Insert and Update, so the field list is written
    /// once. Add a column to the table and you change this in one place.
    /// </summary>
    private static SqlParameter[] BuildParameters(Attraction a) => new[]
    {
        Db.P("@Name",                a.Name.Trim()),
        Db.P("@CategoryId",          a.CategoryId),
        Db.P("@Address",             a.Address),
        Db.P("@City",                a.City),
        Db.P("@ProvinceId",          a.ProvinceId),
        Db.P("@Description",         a.Description),
        Db.P("@Rate",                a.Rate),
        Db.P("@Quantity",            a.Quantity),
        Db.P("@UnitOfMeasure",       a.UnitOfMeasure),
        Db.P("@DurationMinutes",     a.DurationMinutes),
        Db.P("@OperatingTimeStart",  a.OperatingTimeStart),
        Db.P("@OperatingTimeEnd",    a.OperatingTimeEnd),
        Db.P("@BestOperatingSeason", a.BestOperatingSeason)
    };
}
