namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Violation;

public sealed record ViolationDetectedEventSchema(
    Guid AggregateId,
    Guid RuleId,
    Guid SourceReference,
    string Reason,
    string Severity,
    string RecommendedAction,
    DateTimeOffset DetectedAt);

public sealed record ViolationAcknowledgedEventSchema(
    Guid AggregateId,
    DateTimeOffset AcknowledgedAt);

public sealed record ViolationResolvedEventSchema(
    Guid AggregateId,
    string Resolution,
    DateTimeOffset ResolvedAt);
