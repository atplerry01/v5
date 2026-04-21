using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;

public sealed record CreateServiceOptionCommand(
    Guid ServiceOptionId,
    Guid ServiceDefinitionId,
    string Code,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => ServiceOptionId;
}

public sealed record UpdateServiceOptionCommand(
    Guid ServiceOptionId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => ServiceOptionId;
}

public sealed record ActivateServiceOptionCommand(Guid ServiceOptionId) : IHasAggregateId
{
    public Guid AggregateId => ServiceOptionId;
}

public sealed record ArchiveServiceOptionCommand(Guid ServiceOptionId) : IHasAggregateId
{
    public Guid AggregateId => ServiceOptionId;
}
