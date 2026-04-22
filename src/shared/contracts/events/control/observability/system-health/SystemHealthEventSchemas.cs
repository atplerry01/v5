namespace Whycespace.Shared.Contracts.Events.Control.Observability.SystemHealth;

public sealed record SystemHealthEvaluatedEventSchema(
    Guid AggregateId,
    string ComponentName,
    string Status,
    DateTimeOffset EvaluatedAt);

public sealed record SystemHealthDegradedEventSchema(
    Guid AggregateId,
    string NewStatus,
    string Reason,
    DateTimeOffset OccurredAt);

public sealed record SystemHealthRestoredEventSchema(
    Guid AggregateId,
    DateTimeOffset RestoredAt);
