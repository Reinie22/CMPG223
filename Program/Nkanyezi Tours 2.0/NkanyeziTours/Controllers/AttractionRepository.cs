using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class AttractionRepository
{
    private readonly Db _db;

    public AttractionRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT  a.*,
                c.CategoryName,
                p.ProvinceName
        FROM        dbo.Attraction         AS a
        LEFT JOIN   dbo.AttractionCategory AS c ON c.CategoryId  = a.CategoryId
        LEFT JOIN   dbo.Province           AS p ON p.ProvinceId  = a.ProvinceId";

    public List<Attraction> GetAll(bool includeInactive = false)
        => _db.Query(SelectSql + @"
        WHERE   (@IncludeInactive = 1 OR a.IsActive = 1)
        ORDER BY a.Name;",
        Map,
        Db.P("@IncludeInactive", includeInactive));

    public Attraction? GetById(int attractionId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   a.AttractionId = @AttractionId;",
        Map,
        Db.P("@AttractionId", attractionId));

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

    public bool NameExists(string name, int? excludeAttractionId = null)
        => _db.Scalar<int>(@"
        SELECT  COUNT(*)
        FROM    dbo.Attraction
        WHERE   Name = @Name
          AND   (@ExcludeId IS NULL OR AttractionId <> @ExcludeId);",
        Db.P("@Name",      name.Trim()),
        Db.P("@ExcludeId", excludeAttractionId)) > 0;

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

    public void SetImageUrl(int attractionId, string? imageUrl)
        => _db.Execute(@"
        UPDATE  dbo.Attraction
        SET     ImageUrl = @ImageUrl
        WHERE   AttractionId = @AttractionId;",
        Db.P("@AttractionId", attractionId),
        Db.P("@ImageUrl",     imageUrl));

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

    public bool Delete(int attractionId)
        => _db.Scalar<bool>(@"
        DECLARE @HasBookings BIT =
            CASE WHEN EXISTS (SELECT 1
                              FROM   dbo.AttractionBooking
                              WHERE  AttractionId = @AttractionId)
                   OR EXISTS (SELECT 1
                              FROM   dbo.TripRequestItem
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

    public void Reactivate(int attractionId)
        => _db.Execute(@"
        UPDATE  dbo.Attraction
        SET     IsActive = 1
        WHERE   AttractionId = @AttractionId;",
        Db.P("@AttractionId", attractionId));

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
        CreatedDate         = (DateTime)r["CreatedDate"],
        ImageUrl            = Db.GetOptionalString(r, "ImageUrl")
    };

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
