using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;

public sealed record CreateServiceOfferingCommand(
    Guid ServiceOfferingId,
    string Name,
    Guid ServiceDefinitionId,
    Guid? PackageId) : IHasAggregateId
{
    public Guid AggregateId => ServiceOfferingId;
}

public sealed record UpdateServiceOfferingCommand(
    Guid ServiceOfferingId,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => ServiceOfferingId;
}

public sealed record ActivateServiceOfferingCommand(Guid ServiceOfferingId) : IHasAggregateId
{
    public Guid AggregateId => ServiceOfferingId;
}

public sealed record ArchiveServiceOfferingCommand(Guid ServiceOfferingId) : IHasAggregateId
{
    public Guid AggregateId => ServiceOfferingId;
}
