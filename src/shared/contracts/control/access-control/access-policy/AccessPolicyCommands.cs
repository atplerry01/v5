using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;

public sealed record DefineAccessPolicyCommand(
    Guid PolicyId,
    string Name,
    string Scope,
    IReadOnlyList<string> AllowedRoleIds) : IHasAggregateId
{
    public Guid AggregateId => PolicyId;
}

public sealed record ActivateAccessPolicyCommand(
    Guid PolicyId) : IHasAggregateId
{
    public Guid AggregateId => PolicyId;
}

public sealed record RetireAccessPolicyCommand(
    Guid PolicyId) : IHasAggregateId
{
    public Guid AggregateId => PolicyId;
}
