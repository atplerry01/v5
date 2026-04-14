namespace Whycespace.Shared.Contracts.Events.Economic.Transaction.Settlement;

public sealed record SettlementInitiatedEventSchema(
    Guid AggregateId,
    decimal Amount,
    string Currency,
    string SourceReference,
    string Provider);

public sealed record SettlementProcessingStartedEventSchema(
    Guid AggregateId);

public sealed record SettlementCompletedEventSchema(
    Guid AggregateId,
    string ExternalReferenceId);

public sealed record SettlementFailedEventSchema(
    Guid AggregateId,
    string Reason);
