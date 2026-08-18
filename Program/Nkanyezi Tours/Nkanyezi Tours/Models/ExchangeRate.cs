namespace Nkanyezi.Models;

/// <summary>
/// One captured exchange rate.
///
/// The project scope promises international clients a currency converter. Rates
/// are captured by hand by the Administrator rather than fetched from an online
/// service, so the system keeps working without an internet connection during
/// the demonstration and there is nothing to pay for.
///
/// Old rows are never deleted. A rate is stored with the date it applies from,
/// and the newest rate on or before a given date is the one used - which means
/// a conversion done last month can still be explained months later.
/// </summary>
public class ExchangeRate
{
    public int ExchangeRateId { get; set; }

    /// <summary>USD, EUR, GBP. Three characters, stored as NCHAR(3).</summary>
    public string CurrencyCode { get; set; } = "";

    public string CurrencyName { get; set; } = "";

    /// <summary>
    /// One unit of this currency equals this many Rand.
    /// So USD at 18.50 means one dollar buys R18.50.
    ///
    /// Six decimal places, because currencies like the yen need them and
    /// rounding early makes a large conversion visibly wrong.
    /// </summary>
    public decimal RateToZar { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public int? CapturedBy { get; set; }
    public string? CapturedByName { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>"USD - US Dollar" for the dropdowns.</summary>
    public string Display => $"{CurrencyCode.Trim()} - {CurrencyName}";

    /// <summary>"1 USD = R18.50" for the converter screen.</summary>
    public string RateDisplay => $"1 {CurrencyCode.Trim()} = R{RateToZar:N2}";

    /// <summary>Rand for a given amount of this currency.</summary>
    public decimal ToZar(decimal amount) => Math.Round(amount * RateToZar, 2);

    /// <summary>This currency for a given amount of Rand.</summary>
    public decimal FromZar(decimal rand) =>
        RateToZar == 0 ? 0 : Math.Round(rand / RateToZar, 2);
}
