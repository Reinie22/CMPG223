using System.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

public class ExchangeRateRepository
{
    private readonly Db _db;

    public ExchangeRateRepository(Db db)
    {
        _db = db;
    }

    private const string SelectSql = @"
        SELECT      x.*,
                    CASE WHEN u.UserId IS NULL THEN NULL
                         ELSE u.Name + ' ' + u.Surname END AS CapturedByName
        FROM        dbo.ExchangeRate AS x
        LEFT JOIN   dbo.AppUser      AS u ON u.UserId = x.CapturedBy";

    public List<ExchangeRate> GetAll()
        => _db.Query(SelectSql + @"
        ORDER BY x.CurrencyCode, x.EffectiveFrom DESC;",
        Map);

    public List<ExchangeRate> GetHistory(string currencyCode)
        => _db.Query(SelectSql + @"
        WHERE   x.CurrencyCode = @CurrencyCode
        ORDER BY x.EffectiveFrom DESC;",
        Map,
        Db.P("@CurrencyCode", currencyCode.Trim()));

    public ExchangeRate? GetById(int exchangeRateId)
        => _db.QuerySingle(SelectSql + @"
        WHERE   x.ExchangeRateId = @ExchangeRateId;",
        Map,
        Db.P("@ExchangeRateId", exchangeRateId));

    public ExchangeRate? GetRateOn(string currencyCode, DateTime date)
        => _db.QuerySingle(@"
        SELECT      TOP 1 x.*,
                    CASE WHEN u.UserId IS NULL THEN NULL
                         ELSE u.Name + ' ' + u.Surname END AS CapturedByName
        FROM        dbo.ExchangeRate AS x
        LEFT JOIN   dbo.AppUser      AS u ON u.UserId = x.CapturedBy
        WHERE       x.CurrencyCode  = @CurrencyCode
          AND       x.EffectiveFrom <= @Date
        ORDER BY    x.EffectiveFrom DESC;",
        Map,
        Db.P("@CurrencyCode", currencyCode.Trim()),
        Db.P("@Date",         date.Date));

    public ExchangeRate? GetCurrentRate(string currencyCode)
        => GetRateOn(currencyCode, DateTime.Today);

    public List<ExchangeRate> GetCurrentRates()
        => _db.Query(@"
        SELECT      x.*,
                    CASE WHEN u.UserId IS NULL THEN NULL
                         ELSE u.Name + ' ' + u.Surname END AS CapturedByName
        FROM        dbo.ExchangeRate AS x
        LEFT JOIN   dbo.AppUser      AS u ON u.UserId = x.CapturedBy
        WHERE       x.EffectiveFrom = (
                        SELECT  MAX(x2.EffectiveFrom)
                        FROM    dbo.ExchangeRate AS x2
                        WHERE   x2.CurrencyCode   = x.CurrencyCode
                          AND   x2.EffectiveFrom <= @Today)
        ORDER BY    x.CurrencyCode;",
        Map,
        Db.P("@Today", DateTime.Today));

    public int Capture(ExchangeRate rate)
        => _db.ExecuteInsert(@"
        UPDATE  dbo.ExchangeRate
        SET     CurrencyName = @CurrencyName,
                RateToZar    = @RateToZar,
                CapturedBy   = @CapturedBy
        WHERE   CurrencyCode  = @CurrencyCode
          AND   EffectiveFrom = @EffectiveFrom;

        IF @@ROWCOUNT = 0
        BEGIN
            INSERT INTO dbo.ExchangeRate (CurrencyCode,  CurrencyName,  RateToZar,
                                          EffectiveFrom,  CapturedBy)
            VALUES                       (@CurrencyCode, @CurrencyName, @RateToZar,
                                          @EffectiveFrom, @CapturedBy);

            SELECT CAST(SCOPE_IDENTITY() AS INT);
        END
        ELSE
            SELECT  ExchangeRateId
            FROM    dbo.ExchangeRate
            WHERE   CurrencyCode  = @CurrencyCode
              AND   EffectiveFrom = @EffectiveFrom;",
        Db.P("@CurrencyCode",  rate.CurrencyCode.Trim()),
        Db.P("@CurrencyName",  rate.CurrencyName.Trim()),
        Db.P("@RateToZar",     rate.RateToZar),
        Db.P("@EffectiveFrom", rate.EffectiveFrom.Date),
        Db.P("@CapturedBy",    rate.CapturedBy));

    public void Delete(int exchangeRateId)
        => _db.Execute(@"
        DELETE FROM dbo.ExchangeRate WHERE ExchangeRateId = @ExchangeRateId;",
        Db.P("@ExchangeRateId", exchangeRateId));

    private static ExchangeRate Map(SqlDataReader r) => new ExchangeRate
    {
        ExchangeRateId = (int)r["ExchangeRateId"],
        CurrencyCode   = ((string)r["CurrencyCode"]).Trim(),
        CurrencyName   = (string)r["CurrencyName"],
        RateToZar      = (decimal)r["RateToZar"],
        EffectiveFrom  = (DateTime)r["EffectiveFrom"],
        CapturedBy     = Db.GetNullable<int>(r, "CapturedBy"),
        CapturedByName = Db.GetOptionalString(r, "CapturedByName"),
        CreatedDate    = (DateTime)r["CreatedDate"]
    };
}
