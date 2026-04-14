namespace Whycespace.Shared.Contracts.Economic.Transaction.Expense;

public sealed record RecordExpenseCommand(
    Guid ExpenseId,
    decimal Amount,
    string Currency,
    string Category,
    string SourceReference);
