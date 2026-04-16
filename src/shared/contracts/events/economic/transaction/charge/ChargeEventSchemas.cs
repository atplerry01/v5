namespace Whycespace.Shared.Contracts.Events.Economic.Transaction.Charge;

public sealed record ChargeCalculatedEventSchema(
    Guid AggregateId,
    Guid TransactionId,
    string Type,
    decimal BaseAmount,
    decimal ChargeAmount,
    string Currency,
    DateTimeOffset CalculatedAt);

public sealed record ChargeAppliedEventSchema(
    Guid AggregateId,
    Guid TransactionId,
    decimal AppliedAmount,
    DateTimeOffset AppliedAt);
