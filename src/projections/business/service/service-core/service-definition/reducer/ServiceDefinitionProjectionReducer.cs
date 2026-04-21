using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceDefinition;

namespace Whycespace.Projections.Business.Service.ServiceCore.ServiceDefinition.Reducer;

public static class ServiceDefinitionProjectionReducer
{
    public static ServiceDefinitionReadModel Apply(ServiceDefinitionReadModel state, ServiceDefinitionCreatedEventSchema e) =>
        state with
        {
            ServiceDefinitionId = e.AggregateId,
            Name = e.Name,
            Category = e.Category,
            Status = "Draft"
        };

    public static ServiceDefinitionReadModel Apply(ServiceDefinitionReadModel state, ServiceDefinitionUpdatedEventSchema e) =>
        state with
        {
            ServiceDefinitionId = e.AggregateId,
            Name = e.Name,
            Category = e.Category
        };

    public static ServiceDefinitionReadModel Apply(ServiceDefinitionReadModel state, ServiceDefinitionActivatedEventSchema e) =>
        state with
        {
            ServiceDefinitionId = e.AggregateId,
            Status = "Active"
        };

    public static ServiceDefinitionReadModel Apply(ServiceDefinitionReadModel state, ServiceDefinitionArchivedEventSchema e) =>
        state with
        {
            ServiceDefinitionId = e.AggregateId,
            Status = "Archived"
        };
}
