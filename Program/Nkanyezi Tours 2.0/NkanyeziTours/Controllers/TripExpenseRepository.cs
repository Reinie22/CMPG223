using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class TripExpenseRepository
{
    private readonly Db _db;

    public TripExpenseRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      e.*,
                    ec.CategoryName,
                    i.TripName,
                    u.Name + ' ' + u.Surname AS CapturedByName
        FROM        dbo.TripExpense     AS e
        INNER JOIN  dbo.ExpenseCategory AS ec ON ec.ExpenseCategoryId = e.ExpenseCategoryId
        INNER JOIN  dbo.Itinerary       AS i  ON i.ItineraryId        = e.ItineraryId
        INNER JOIN  dbo.AppUser         AS u  ON u.UserId             = e.CapturedBy";

    public List<TripExpense> GetByItinerary(int itineraryId)
        => _db.Query(SelectSql + @"
        WHERE   e.ItineraryId = @ItineraryId
        ORDER BY e.ExpenseDate DESC, e.ExpenseId DESC;",
        Map,
        Db.P("@ItineraryId", itineraryId));

    public TripExpense? GetById(int expenseId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   e.ExpenseId = @ExpenseId;",
        Map,
        Db.P("@ExpenseId", expenseId));

    public List<TripExpense> Search(int? itineraryId = null, int? expenseCategoryId = null,
                                    DateTime? fromDate = null, DateTime? toDate = null,
                                    string? description = null)
        => _db.Query(SelectSql + @"
        WHERE   (@ItineraryId IS NULL OR e.ItineraryId       = @ItineraryId)
          AND   (@CategoryId  IS NULL OR e.ExpenseCategoryId = @CategoryId)
          AND   (@FromDate    IS NULL OR e.ExpenseDate      >= @FromDate)
          AND   (@ToDate      IS NULL OR e.ExpenseDate      <= @ToDate)
          AND   (@Description IS NULL OR e.Description    LIKE @Description ESCAPE '\')
        ORDER BY e.ExpenseDate DESC;",
        Map,
        Db.P("@ItineraryId", itineraryId),
        Db.P("@CategoryId",  expenseCategoryId),
        Db.P("@FromDate",    fromDate?.Date),
        Db.P("@ToDate",      toDate?.Date),
        Db.Like("@Description", description));

    public List<ExpenseCategoryTotal> GetTotalsByCategory(int itineraryId)
        => _db.Query(@"
        SELECT      ec.ExpenseCategoryId,
                    ec.CategoryName,
                    ISNULL(SUM(e.Amount), 0) AS Total,
                    COUNT(e.ExpenseId)       AS LineCount
        FROM        dbo.ExpenseCategory AS ec
        LEFT JOIN   dbo.TripExpense     AS e ON e.ExpenseCategoryId = ec.ExpenseCategoryId
                                            AND e.ItineraryId       = @ItineraryId
        GROUP BY    ec.ExpenseCategoryId, ec.CategoryName
        ORDER BY    ec.CategoryName;",
        r => new ExpenseCategoryTotal
        {
            ExpenseCategoryId = (int)r["ExpenseCategoryId"],
            CategoryName      = (string)r["CategoryName"],
            Total             = (decimal)r["Total"],
            LineCount         = (int)r["LineCount"]
        },
        Db.P("@ItineraryId", itineraryId));

    public decimal GetTotal(int itineraryId)
        => _db.Scalar<decimal>(@"
        SELECT  ISNULL(SUM(Amount), 0)
        FROM    dbo.TripExpense
        WHERE   ItineraryId = @ItineraryId;",
        Db.P("@ItineraryId", itineraryId));

    public int Create(TripExpense expense)
        => _db.ExecuteInsert(@"
        INSERT INTO dbo.TripExpense
              (ItineraryId,  ExpenseCategoryId,  Description,  Amount,
               AmountOriginal,  CurrencyCode,  ExpenseDate,  ReceiptNumber,  CapturedBy)
        VALUES
              (@ItineraryId, @ExpenseCategoryId, @Description, @Amount,
               @AmountOriginal, @CurrencyCode, @ExpenseDate, @ReceiptNumber, @CapturedBy);

        SELECT CAST(SCOPE_IDENTITY() AS INT);",
        BuildParameters(expense));

    public void Update(TripExpense expense)
    {
        var parameters = new List<SqlParameter> { Db.P("@ExpenseId", expense.ExpenseId) };
        parameters.AddRange(BuildParameters(expense));

        _db.Execute(@"
        UPDATE  dbo.TripExpense
        SET     ExpenseCategoryId = @ExpenseCategoryId,
                Description       = @Description,
                Amount            = @Amount,
                AmountOriginal    = @AmountOriginal,
                CurrencyCode      = @CurrencyCode,
                ExpenseDate       = @ExpenseDate,
                ReceiptNumber     = @ReceiptNumber
        WHERE   ExpenseId = @ExpenseId;",
        parameters.ToArray());
    }

    public void Delete(int expenseId)
        => _db.Execute(@"
        DELETE FROM dbo.TripExpense WHERE ExpenseId = @ExpenseId;",
        Db.P("@ExpenseId", expenseId));

    private static TripExpense Map(SqlDataReader r) => new TripExpense
    {
        ExpenseId         = (int)r["ExpenseId"],
        ItineraryId       = (int)r["ItineraryId"],
        TripName          = Db.GetOptionalString(r, "TripName"),
        ExpenseCategoryId = (int)r["ExpenseCategoryId"],
        CategoryName      = Db.GetOptionalString(r, "CategoryName"),
        Description       = (string)r["Description"],
        Amount            = (decimal)r["Amount"],
        AmountOriginal    = Db.GetNullable<decimal>(r, "AmountOriginal"),
        CurrencyCode      = ((string)r["CurrencyCode"]).Trim(),
        ExpenseDate       = (DateTime)r["ExpenseDate"],
        ReceiptNumber     = Db.GetNullableString(r, "ReceiptNumber"),
        CapturedBy        = (int)r["CapturedBy"],
        CapturedByName    = Db.GetOptionalString(r, "CapturedByName"),
        CreatedDate       = (DateTime)r["CreatedDate"]
    };

    private static SqlParameter[] BuildParameters(TripExpense e) => new[]
    {
        Db.P("@ItineraryId",       e.ItineraryId),
        Db.P("@ExpenseCategoryId", e.ExpenseCategoryId),
        Db.P("@Description",       e.Description.Trim()),
        Db.P("@Amount",            e.Amount),
        Db.P("@AmountOriginal",    e.AmountOriginal),
        Db.P("@CurrencyCode",      e.CurrencyCode.Trim()),
        Db.P("@ExpenseDate",       e.ExpenseDate.Date),
        Db.P("@ReceiptNumber",     e.ReceiptNumber),
        Db.P("@CapturedBy",        e.CapturedBy)
    };
}
