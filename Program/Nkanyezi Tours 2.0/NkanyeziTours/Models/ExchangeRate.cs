namespace Nkanyezi.Models;

public class ExchangeRate
{
    public int ExchangeRateId { get; set; }

    public string CurrencyCode { get; set; } = "";

    public string CurrencyName { get; set; } = "";

    public decimal RateToZar { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public int? CapturedBy { get; set; }
    public string? CapturedByName { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Display => $"{CurrencyCode.Trim()} - {CurrencyName}";

    public string RateDisplay => $"1 {CurrencyCode.Trim()} = R{RateToZar:N2}";

    public decimal ToZar(decimal amount) => Math.Round(amount * RateToZar, 2);

    public decimal FromZar(decimal rand) =>
        RateToZar == 0 ? 0 : Math.Round(rand / RateToZar, 2);
}
