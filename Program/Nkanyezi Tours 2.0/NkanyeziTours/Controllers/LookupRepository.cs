using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class LookupRepository
{
    private readonly Db _db;

    public LookupRepository(Db db)
    {
        _db = db;
    }

    public List<Lookup> GetProvinces()
        => _db.Query(@"
        SELECT   ProvinceId AS Id, ProvinceName AS Name
        FROM     dbo.Province
        ORDER BY ProvinceName;", MapLookup);

    public List<Lookup> GetAttractionCategories()
        => _db.Query(@"
        SELECT   CategoryId AS Id, CategoryName AS Name
        FROM     dbo.AttractionCategory
        ORDER BY CategoryName;", MapLookup);

    public List<Lookup> GetExpenseCategories()
        => _db.Query(@"
        SELECT   ExpenseCategoryId AS Id, CategoryName AS Name
        FROM     dbo.ExpenseCategory
        ORDER BY CategoryName;", MapLookup);

    public List<Lookup> GetRoles()
        => _db.Query(@"
        SELECT   RoleId AS Id, RoleName AS Name
        FROM     dbo.Role
        ORDER BY RoleId;", MapLookup);

    public List<string> GetRouteCities()
        => _db.Query(@"
        SELECT DepartureCity AS City FROM dbo.Route WHERE IsActive = 1
        UNION
        SELECT ArrivalCity   AS City FROM dbo.Route WHERE IsActive = 1
        ORDER BY City;",
        r => (string)r["City"]);

    public List<string> GetAccommodationTypes()
        => _db.Query(@"
        SELECT DISTINCT Type
        FROM     dbo.Accommodation
        WHERE    Type IS NOT NULL
        ORDER BY Type;",
        r => (string)r["Type"]);

    private static Lookup MapLookup(SqlDataReader r) => new Lookup
    {
        Id   = (int)r["Id"],
        Name = (string)r["Name"]
    };
}
