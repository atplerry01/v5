using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceLevel;

namespace Whycespace.Projections.Business.Service.ServiceCore.ServiceLevel.Reducer;

public static class ServiceLevelProjectionReducer
{
    public static ServiceLevelReadModel Apply(ServiceLevelReadModel state, ServiceLevelCreatedEventSchema e) =>
        state with
        {
            ServiceLevelId = e.AggregateId,
            ServiceDefinitionId = e.ServiceDefinitionId,
            Code = e.Code,
            Name = e.Name,
            Target = e.Target,
            Status = "Draft"
        };

    public static ServiceLevelReadModel Apply(ServiceLevelReadModel state, ServiceLevelUpdatedEventSchema e) =>
        state with
        {
            ServiceLevelId = e.AggregateId,
            Name = e.Name,
            Target = e.Target
        };

    public static ServiceLevelReadModel Apply(ServiceLevelReadModel state, ServiceLevelActivatedEventSchema e) =>
        state with
        {
            ServiceLevelId = e.AggregateId,
            Status = "Active"
        };

    public static ServiceLevelReadModel Apply(ServiceLevelReadModel state, ServiceLevelArchivedEventSchema e) =>
        state with
        {
            ServiceLevelId = e.AggregateId,
            Status = "Archived"
        };
}
