using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Violation;

public sealed record DetectViolationCommand(
    Guid ViolationId,
    Guid RuleId,
    Guid SourceReference,
    string Reason,
    string Severity,
    string RecommendedAction,
    DateTimeOffset DetectedAt) : IHasAggregateId
{
    public Guid AggregateId => ViolationId;
}

public sealed record AcknowledgeViolationCommand(
    Guid ViolationId,
    DateTimeOffset AcknowledgedAt) : IHasAggregateId
{
    public Guid AggregateId => ViolationId;
}

public sealed record ResolveViolationCommand(
    Guid ViolationId,
    string Resolution,
    DateTimeOffset ResolvedAt) : IHasAggregateId
{
    public Guid AggregateId => ViolationId;
}
