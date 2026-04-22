using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;

public sealed record InitiateDiscrepancyResolutionCommand(
    Guid ResolutionId,
    string DetectionId,
    DateTimeOffset InitiatedAt) : IHasAggregateId
{
    public Guid AggregateId => ResolutionId;
}

public sealed record CompleteDiscrepancyResolutionCommand(
    Guid ResolutionId,
    string Outcome,
    string Notes,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => ResolutionId;
}
