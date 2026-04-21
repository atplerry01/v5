using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;

public sealed record CreateServiceConstraintCommand(
    Guid ServiceConstraintId,
    Guid ServiceDefinitionId,
    int Kind,
    string Descriptor) : IHasAggregateId
{
    public Guid AggregateId => ServiceConstraintId;
}

public sealed record UpdateServiceConstraintCommand(
    Guid ServiceConstraintId,
    int Kind,
    string Descriptor) : IHasAggregateId
{
    public Guid AggregateId => ServiceConstraintId;
}

public sealed record ActivateServiceConstraintCommand(Guid ServiceConstraintId) : IHasAggregateId
{
    public Guid AggregateId => ServiceConstraintId;
}

public sealed record ArchiveServiceConstraintCommand(Guid ServiceConstraintId) : IHasAggregateId
{
    public Guid AggregateId => ServiceConstraintId;
}
