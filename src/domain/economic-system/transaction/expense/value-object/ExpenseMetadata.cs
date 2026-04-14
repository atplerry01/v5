namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

public sealed record ExpenseMetadata(
    string Currency,
    string? Description,
    string? Memo)
{
    public static ExpenseMetadata Of(string currency, string? description = null, string? memo = null)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));
        return new ExpenseMetadata(currency.Trim(), description, memo);
    }
}
