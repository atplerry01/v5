using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Expense;

namespace Whycespace.Projections.Economic.Transaction.Expense.Reducer;

public static class ExpenseProjectionReducer
{
    public static ExpenseReadModel Apply(ExpenseReadModel state, ExpenseCreatedEventSchema e) =>
        state with
        {
            ExpenseId = e.AggregateId,
            Amount = e.Amount,
            Currency = e.Currency,
            Category = e.Category,
            SourceReference = e.SourceReference,
            Status = "Created"
        };

    public static ExpenseReadModel Apply(ExpenseReadModel state, ExpenseRecordedEventSchema e) =>
        state with
        {
            ExpenseId = e.AggregateId,
            Amount = e.Amount,
            Currency = e.Currency,
            Status = "Recorded"
        };

    public static ExpenseReadModel Apply(ExpenseReadModel state, ExpenseCancelledEventSchema e) =>
        state with
        {
            ExpenseId = e.AggregateId,
            Status = "Cancelled"
        };
}
