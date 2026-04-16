using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Transaction.Expense;

public sealed record RecordExpenseCommand(
    Guid ExpenseId,
    decimal Amount,
    string Currency,
    string Category,
    string SourceReference) : IHasAggregateId
{
    public Guid AggregateId => ExpenseId;
}
