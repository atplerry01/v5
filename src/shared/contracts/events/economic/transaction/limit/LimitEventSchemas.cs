namespace Whycespace.Shared.Contracts.Events.Economic.Transaction.Limit;

public sealed record LimitDefinedEventSchema(
    Guid AggregateId,
    Guid AccountId,
    string Type,
    decimal Threshold,
    string Currency,
    DateTimeOffset DefinedAt);

public sealed record LimitCheckedEventSchema(
    Guid AggregateId,
    Guid TransactionId,
    decimal TransactionAmount,
    decimal CurrentUtilization,
    DateTimeOffset CheckedAt);

public sealed record LimitExceededEventSchema(
    Guid AggregateId,
    Guid TransactionId,
    decimal TransactionAmount,
    decimal Threshold,
    DateTimeOffset ExceededAt);
