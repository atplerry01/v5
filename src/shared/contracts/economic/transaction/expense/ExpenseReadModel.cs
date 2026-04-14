namespace Whycespace.Shared.Contracts.Economic.Transaction.Expense;

public sealed record ExpenseReadModel
{
    public Guid ExpenseId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string SourceReference { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
