using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;

public sealed record InitiateConsistencyCheckCommand(
    Guid CheckId,
    string ScopeTarget,
    DateTimeOffset InitiatedAt) : IHasAggregateId
{
    public Guid AggregateId => CheckId;
}

public sealed record CompleteConsistencyCheckCommand(
    Guid CheckId,
    bool HasDiscrepancies,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => CheckId;
}
