using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;

public sealed record CreateServiceDefinitionCommand(
    Guid ServiceDefinitionId,
    string Name,
    string Category) : IHasAggregateId
{
    public Guid AggregateId => ServiceDefinitionId;
}

public sealed record UpdateServiceDefinitionCommand(
    Guid ServiceDefinitionId,
    string Name,
    string Category) : IHasAggregateId
{
    public Guid AggregateId => ServiceDefinitionId;
}

public sealed record ActivateServiceDefinitionCommand(Guid ServiceDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => ServiceDefinitionId;
}

public sealed record ArchiveServiceDefinitionCommand(Guid ServiceDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => ServiceDefinitionId;
}
