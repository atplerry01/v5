using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceOption;

namespace Whycespace.Projections.Business.Service.ServiceCore.ServiceOption.Reducer;

public static class ServiceOptionProjectionReducer
{
    public static ServiceOptionReadModel Apply(ServiceOptionReadModel state, ServiceOptionCreatedEventSchema e) =>
        state with
        {
            ServiceOptionId = e.AggregateId,
            ServiceDefinitionId = e.ServiceDefinitionId,
            Code = e.Code,
            Name = e.Name,
            Kind = e.Kind,
            Status = "Draft"
        };

    public static ServiceOptionReadModel Apply(ServiceOptionReadModel state, ServiceOptionUpdatedEventSchema e) =>
        state with
        {
            ServiceOptionId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind
        };

    public static ServiceOptionReadModel Apply(ServiceOptionReadModel state, ServiceOptionActivatedEventSchema e) =>
        state with
        {
            ServiceOptionId = e.AggregateId,
            Status = "Active"
        };

    public static ServiceOptionReadModel Apply(ServiceOptionReadModel state, ServiceOptionArchivedEventSchema e) =>
        state with
        {
            ServiceOptionId = e.AggregateId,
            Status = "Archived"
        };
}
