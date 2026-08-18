using Microsoft.Data.SqlClient;
using Nkanyezi.Models;

namespace Nkanyezi.Data;

/// <summary>
/// The captured exchange rates behind the currency converter.
///
/// Rates are entered by hand by the Administrator rather than fetched from an
/// online service, so the system keeps working without an internet connection
/// during the demonstration and there is nothing to pay for.
///
/// Rates are never edited or deleted. A new rate is captured with the date it
/// applies from, and the newest rate on or before a given date is the one used.
/// That means a conversion done last month can still be explained today, which
/// editing the old row in place would make impossible.
/// </summary>
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

    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

    /// <summary>Every rate ever captured, newest first. The history screen.</summary>
    public List<ExchangeRate> GetAll()
        => _db.Query(SelectSql + @"
        ORDER BY x.CurrencyCode, x.EffectiveFrom DESC;",
        Map);

    /// <summary>The history for one currency.</summary>
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

    /// <summary>
    /// The rate to use for one currency on one date: the newest rate captured
    /// on or before that date. Returns null when no rate had been captured yet,
    /// which the converter must handle rather than assuming 1.
    ///
    /// TOP 1 with ORDER BY DESC is the whole trick. Without the ORDER BY, TOP 1
    /// returns whichever row SQL Server happens to reach first, which is not
    /// necessarily the newest - and the bug only shows up months later when
    /// there are several rates to choose between.
    /// </summary>
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

    /// <summary>Today's rate for one currency.</summary>
    public ExchangeRate? GetCurrentRate(string currencyCode)
        => GetRateOn(currencyCode, DateTime.Today);

    /// <summary>
    /// The current rate for every currency, one row each, for the converter's
    /// dropdown and the rate list screen.
    ///
    /// The subquery picks the newest EffectiveFrom per currency and the outer
    /// query then matches that exact row. This is the standard way to get
    /// "the latest row per group" in SQL Server and it is worth learning - the
    /// same shape solves a dozen problems in the reports.
    /// </summary>
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

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Captures a rate.
    ///
    /// The database allows only one rate per currency per day, so capturing a
    /// second rate for a day that already has one replaces it rather than
    /// failing - which is what an administrator correcting a typo actually
    /// wants. Any other day's rate is left alone, so the history survives.
    ///
    /// Returns the id of the row, new or updated.
    /// </summary>
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

    /// <summary>
    /// Removes a captured rate. Only for a rate typed in by mistake - deleting
    /// a rate that has been used to convert something makes that conversion
    /// unexplainable afterwards.
    /// </summary>
    public void Delete(int exchangeRateId)
        => _db.Execute(@"
        DELETE FROM dbo.ExchangeRate WHERE ExchangeRateId = @ExchangeRateId;",
        Db.P("@ExchangeRateId", exchangeRateId));

    // -------------------------------------------------------------------------

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
