namespace Whycespace.Shared.Contracts.Events.Economic.Transaction.Expense;

public sealed record ExpenseCreatedEventSchema(
    Guid AggregateId,
    decimal Amount,
    string Currency,
    string Category,
    string SourceReference);

public sealed record ExpenseRecordedEventSchema(
    Guid AggregateId,
    decimal Amount,
    string Currency);

public sealed record ExpenseCancelledEventSchema(
    Guid AggregateId,
    string Reason);
