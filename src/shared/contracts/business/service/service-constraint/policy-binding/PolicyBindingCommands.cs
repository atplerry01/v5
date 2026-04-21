using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;

public sealed record CreatePolicyBindingCommand(
    Guid PolicyBindingId,
    Guid ServiceDefinitionId,
    string PolicyRef,
    int Scope) : IHasAggregateId
{
    public Guid AggregateId => PolicyBindingId;
}

public sealed record BindPolicyBindingCommand(
    Guid PolicyBindingId,
    DateTimeOffset BoundAt) : IHasAggregateId
{
    public Guid AggregateId => PolicyBindingId;
}

public sealed record UnbindPolicyBindingCommand(
    Guid PolicyBindingId,
    DateTimeOffset UnboundAt) : IHasAggregateId
{
    public Guid AggregateId => PolicyBindingId;
}

public sealed record ArchivePolicyBindingCommand(Guid PolicyBindingId) : IHasAggregateId
{
    public Guid AggregateId => PolicyBindingId;
}
