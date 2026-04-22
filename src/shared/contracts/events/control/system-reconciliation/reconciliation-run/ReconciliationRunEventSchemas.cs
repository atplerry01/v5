namespace Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ReconciliationRun;

public sealed record ReconciliationRunScheduledEventSchema(
    Guid AggregateId,
    string Scope);

public sealed record ReconciliationRunStartedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartedAt);

public sealed record ReconciliationRunCompletedEventSchema(
    Guid AggregateId,
    int ChecksProcessed,
    int DiscrepanciesFound,
    DateTimeOffset CompletedAt);

public sealed record ReconciliationRunAbortedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset AbortedAt);
