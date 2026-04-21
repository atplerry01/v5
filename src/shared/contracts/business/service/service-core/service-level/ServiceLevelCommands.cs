using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;

public sealed record CreateServiceLevelCommand(
    Guid ServiceLevelId,
    Guid ServiceDefinitionId,
    string Code,
    string Name,
    string Target) : IHasAggregateId
{
    public Guid AggregateId => ServiceLevelId;
}

public sealed record UpdateServiceLevelCommand(
    Guid ServiceLevelId,
    string Name,
    string Target) : IHasAggregateId
{
    public Guid AggregateId => ServiceLevelId;
}

public sealed record ActivateServiceLevelCommand(Guid ServiceLevelId) : IHasAggregateId
{
    public Guid AggregateId => ServiceLevelId;
}

public sealed record ArchiveServiceLevelCommand(Guid ServiceLevelId) : IHasAggregateId
{
    public Guid AggregateId => ServiceLevelId;
}
