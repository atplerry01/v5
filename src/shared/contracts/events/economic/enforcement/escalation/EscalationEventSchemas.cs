namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Escalation;

public sealed record EscalationInitializedEventSchema(
    Guid AggregateId,
    DateTimeOffset WindowStart,
    long WindowDurationTicks,
    DateTimeOffset InitializedAt);

public sealed record ViolationAccumulatedEventSchema(
    Guid AggregateId,
    Guid ViolationId,
    int SeverityWeight,
    int NewCount,
    int NewSeverityScore,
    DateTimeOffset AccumulatedAt);

public sealed record EscalationLevelIncreasedEventSchema(
    Guid AggregateId,
    string PreviousLevel,
    string NewLevel,
    int Count,
    int SeverityScore,
    DateTimeOffset EscalatedAt);

public sealed record EscalationResetEventSchema(
    Guid AggregateId,
    DateTimeOffset NewWindowStart,
    long NewWindowDurationTicks,
    DateTimeOffset ResetAt);
