using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceConstraint;

namespace Whycespace.Projections.Business.Service.ServiceConstraint.ServiceConstraint.Reducer;

public static class ServiceConstraintProjectionReducer
{
    public static ServiceConstraintReadModel Apply(ServiceConstraintReadModel state, ServiceConstraintCreatedEventSchema e) =>
        state with
        {
            ServiceConstraintId = e.AggregateId,
            ServiceDefinitionId = e.ServiceDefinitionId,
            Kind = e.Kind,
            Descriptor = e.Descriptor,
            Status = "Draft"
        };

    public static ServiceConstraintReadModel Apply(ServiceConstraintReadModel state, ServiceConstraintUpdatedEventSchema e) =>
        state with
        {
            ServiceConstraintId = e.AggregateId,
            Kind = e.Kind,
            Descriptor = e.Descriptor
        };

    public static ServiceConstraintReadModel Apply(ServiceConstraintReadModel state, ServiceConstraintActivatedEventSchema e) =>
        state with
        {
            ServiceConstraintId = e.AggregateId,
            Status = "Active"
        };

    public static ServiceConstraintReadModel Apply(ServiceConstraintReadModel state, ServiceConstraintArchivedEventSchema e) =>
        state with
        {
            ServiceConstraintId = e.AggregateId,
            Status = "Archived"
        };
}
