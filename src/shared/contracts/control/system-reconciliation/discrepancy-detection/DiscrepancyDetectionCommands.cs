using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;

public sealed record DetectDiscrepancyCommand(
    Guid DetectionId,
    string Kind,
    string SourceReference,
    DateTimeOffset DetectedAt) : IHasAggregateId
{
    public Guid AggregateId => DetectionId;
}

public sealed record DismissDiscrepancyCommand(
    Guid DetectionId,
    string Reason,
    DateTimeOffset DismissedAt) : IHasAggregateId
{
    public Guid AggregateId => DetectionId;
}
