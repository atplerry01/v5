namespace Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ConsistencyCheck;

public sealed record ConsistencyCheckInitiatedEventSchema(
    Guid AggregateId,
    string ScopeTarget,
    DateTimeOffset InitiatedAt);

public sealed record ConsistencyCheckCompletedEventSchema(
    Guid AggregateId,
    bool HasDiscrepancies,
    DateTimeOffset CompletedAt);
