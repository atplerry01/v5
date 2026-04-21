using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.PolicyBinding;

namespace Whycespace.Projections.Business.Service.ServiceConstraint.PolicyBinding.Reducer;

public static class PolicyBindingProjectionReducer
{
    public static PolicyBindingReadModel Apply(PolicyBindingReadModel state, PolicyBindingCreatedEventSchema e) =>
        state with
        {
            PolicyBindingId = e.AggregateId,
            ServiceDefinitionId = e.ServiceDefinitionId,
            PolicyRef = e.PolicyRef,
            Scope = e.Scope,
            Status = "Draft"
        };

    public static PolicyBindingReadModel Apply(PolicyBindingReadModel state, PolicyBindingBoundEventSchema e) =>
        state with
        {
            PolicyBindingId = e.AggregateId,
            Status = "Bound",
            BoundAt = e.BoundAt,
            LastUpdatedAt = e.BoundAt
        };

    public static PolicyBindingReadModel Apply(PolicyBindingReadModel state, PolicyBindingUnboundEventSchema e) =>
        state with
        {
            PolicyBindingId = e.AggregateId,
            Status = "Unbound",
            UnboundAt = e.UnboundAt,
            LastUpdatedAt = e.UnboundAt
        };

    public static PolicyBindingReadModel Apply(PolicyBindingReadModel state, PolicyBindingArchivedEventSchema e) =>
        state with
        {
            PolicyBindingId = e.AggregateId,
            Status = "Archived"
        };
}
