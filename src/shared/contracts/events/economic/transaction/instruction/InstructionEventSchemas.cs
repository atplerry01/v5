namespace Whycespace.Shared.Contracts.Events.Economic.Transaction.Instruction;

public sealed record TransactionInstructionCreatedEventSchema(
    Guid AggregateId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    string Type,
    DateTimeOffset CreatedAt);

public sealed record TransactionInstructionExecutedEventSchema(
    Guid AggregateId,
    DateTimeOffset ExecutedAt);

public sealed record TransactionInstructionCancelledEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset CancelledAt);
