using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;

public sealed record CreateServiceWindowCommand(
    Guid ServiceWindowId,
    Guid ServiceDefinitionId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt) : IHasAggregateId
{
    public Guid AggregateId => ServiceWindowId;
}

public sealed record UpdateServiceWindowCommand(
    Guid ServiceWindowId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt) : IHasAggregateId
{
    public Guid AggregateId => ServiceWindowId;
}

public sealed record ActivateServiceWindowCommand(Guid ServiceWindowId) : IHasAggregateId
{
    public Guid AggregateId => ServiceWindowId;
}

public sealed record ArchiveServiceWindowCommand(Guid ServiceWindowId) : IHasAggregateId
{
    public Guid AggregateId => ServiceWindowId;
}
