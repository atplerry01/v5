using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.ServiceOffering;

namespace Whycespace.Projections.Business.Offering.CatalogCore.ServiceOffering.Reducer;

public static class ServiceOfferingProjectionReducer
{
    public static ServiceOfferingReadModel Apply(ServiceOfferingReadModel state, ServiceOfferingCreatedEventSchema e) =>
        state with
        {
            ServiceOfferingId = e.AggregateId,
            Name = e.Name,
            ServiceDefinitionId = e.ServiceDefinitionId,
            PackageId = e.PackageId,
            Status = "Draft"
        };

    public static ServiceOfferingReadModel Apply(ServiceOfferingReadModel state, ServiceOfferingUpdatedEventSchema e) =>
        state with
        {
            ServiceOfferingId = e.AggregateId,
            Name = e.Name
        };

    public static ServiceOfferingReadModel Apply(ServiceOfferingReadModel state, ServiceOfferingActivatedEventSchema e) =>
        state with
        {
            ServiceOfferingId = e.AggregateId,
            Status = "Active"
        };

    public static ServiceOfferingReadModel Apply(ServiceOfferingReadModel state, ServiceOfferingArchivedEventSchema e) =>
        state with
        {
            ServiceOfferingId = e.AggregateId,
            Status = "Archived"
        };
}
