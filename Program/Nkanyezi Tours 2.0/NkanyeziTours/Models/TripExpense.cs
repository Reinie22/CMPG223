namespace Nkanyezi.Models;

public class TripExpense
{
    public int ExpenseId { get; set; }
    public int ItineraryId { get; set; }

    public string? TripName { get; set; }

    public int ExpenseCategoryId { get; set; }
    public string? CategoryName { get; set; }

    public string Description { get; set; } = "";

    public decimal Amount { get; set; }

    public decimal? AmountOriginal { get; set; }

    public string CurrencyCode { get; set; } = "ZAR";

    public DateTime ExpenseDate { get; set; }
    public string? ReceiptNumber { get; set; }

    public int CapturedBy { get; set; }
    public string? CapturedByName { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsForeignCurrency => CurrencyCode.Trim() != "ZAR";

    public string AmountDisplay =>
        IsForeignCurrency && AmountOriginal != null
            ? $"R{Amount:N2} ({CurrencyCode.Trim()} {AmountOriginal:N2})"
            : $"R{Amount:N2}";
}

public class ExpenseCategoryTotal
{
    public int ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal Total { get; set; }
    public int LineCount { get; set; }
}
