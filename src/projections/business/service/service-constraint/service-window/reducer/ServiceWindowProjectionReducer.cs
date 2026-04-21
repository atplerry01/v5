using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceWindow;

namespace Whycespace.Projections.Business.Service.ServiceConstraint.ServiceWindow.Reducer;

public static class ServiceWindowProjectionReducer
{
    public static ServiceWindowReadModel Apply(ServiceWindowReadModel state, ServiceWindowCreatedEventSchema e) =>
        state with
        {
            ServiceWindowId = e.AggregateId,
            ServiceDefinitionId = e.ServiceDefinitionId,
            StartsAt = e.StartsAt,
            EndsAt = e.EndsAt,
            Status = "Draft"
        };

    public static ServiceWindowReadModel Apply(ServiceWindowReadModel state, ServiceWindowUpdatedEventSchema e) =>
        state with
        {
            ServiceWindowId = e.AggregateId,
            StartsAt = e.StartsAt,
            EndsAt = e.EndsAt
        };

    public static ServiceWindowReadModel Apply(ServiceWindowReadModel state, ServiceWindowActivatedEventSchema e) =>
        state with
        {
            ServiceWindowId = e.AggregateId,
            Status = "Active"
        };

    public static ServiceWindowReadModel Apply(ServiceWindowReadModel state, ServiceWindowArchivedEventSchema e) =>
        state with
        {
            ServiceWindowId = e.AggregateId,
            Status = "Archived"
        };
}
