using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// Fills the dropdown lists. Used by nearly every screen in the system.
///
/// The lookup tables all have the same shape - an id and a name - so every
/// method here selects them into the same small Lookup model and the views can
/// bind to any of them the same way.
///
/// Note that the column names differ between the tables (CategoryId, ProvinceId
/// and so on), which is why each query renames them to Id and Name with AS.
/// Doing the renaming in SQL means one MapLookup method serves all of them.
/// </summary>
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

    /// <summary>
    /// The cities that actually appear on routes, for the portal's "where do
    /// you want to go" dropdowns. Built from the data rather than a lookup
    /// table, because the group decided routes are captured freehand.
    ///
    /// UNION rather than UNION ALL on purpose: it removes the duplicates, so a
    /// city that is both a departure and an arrival appears once.
    /// </summary>
    public List<string> GetRouteCities()
        => _db.Query(@"
        SELECT DepartureCity AS City FROM dbo.Route WHERE IsActive = 1
        UNION
        SELECT ArrivalCity   AS City FROM dbo.Route WHERE IsActive = 1
        ORDER BY City;",
        r => (string)r["City"]);

    /// <summary>The distinct accommodation types already captured, for a filter.</summary>
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
