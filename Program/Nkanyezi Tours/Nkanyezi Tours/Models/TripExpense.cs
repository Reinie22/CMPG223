namespace Nkanyezi.Models;

/// <summary>
/// One row of the TripExpense table: something the client spent while away.
///
/// Expenses and bookings are deliberately separate. Bookings are what the
/// agency sold; expenses are what the client spent along the way. They are only
/// added together on the trip summary screen. Storing them in one table would
/// double count revenue in the reports.
///
/// There is no approval step - agents and clients may both capture expenses and
/// nobody signs them off. CapturedBy still records who entered each line, which
/// is enough to see where a figure came from.
/// </summary>
public class TripExpense
{
    public int ExpenseId { get; set; }
    public int ItineraryId { get; set; }

    /// <summary>Joined in for display on the client's own expense list.</summary>
    public string? TripName { get; set; }

    public int ExpenseCategoryId { get; set; }
    public string? CategoryName { get; set; }   // joined in from ExpenseCategory

    public string Description { get; set; } = "";

    /// <summary>
    /// Always in Rand. This is the figure the summary and the reports add up.
    /// When the client paid in another currency, this is the converted amount
    /// and AmountOriginal holds what they actually handed over.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>What the client paid, in the currency they paid it in.</summary>
    public decimal? AmountOriginal { get; set; }

    /// <summary>ZAR unless the client paid in something else.</summary>
    public string CurrencyCode { get; set; } = "ZAR";

    public DateTime ExpenseDate { get; set; }
    public string? ReceiptNumber { get; set; }

    /// <summary>The agent or the client who entered this line. Required.</summary>
    public int CapturedBy { get; set; }
    public string? CapturedByName { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsForeignCurrency => CurrencyCode.Trim() != "ZAR";

    /// <summary>
    /// "R450.00", or "R450.00 (USD 25.00)" when it was paid in something else,
    /// so the client can recognise the amount on their own bank statement.
    /// </summary>
    public string AmountDisplay =>
        IsForeignCurrency && AmountOriginal != null
            ? $"R{Amount:N2} ({CurrencyCode.Trim()} {AmountOriginal:N2})"
            : $"R{Amount:N2}";
}

/// <summary>
/// One category with its total, for the "grouped by category" expense list and
/// the trip cost report.
/// </summary>
public class ExpenseCategoryTotal
{
    public int ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal Total { get; set; }
    public int LineCount { get; set; }
}
