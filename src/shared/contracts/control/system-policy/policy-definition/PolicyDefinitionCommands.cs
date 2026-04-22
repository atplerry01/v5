using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;

public sealed record DefinePolicyCommand(
    Guid PolicyId,
    string Name,
    string ScopeClassification,
    string ScopeActionMask,
    string? ScopeContext = null) : IHasAggregateId
{
    public Guid AggregateId => PolicyId;
}

public sealed record DeprecatePolicyCommand(
    Guid PolicyId) : IHasAggregateId
{
    public Guid AggregateId => PolicyId;
}
