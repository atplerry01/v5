using Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.AccessPolicy;

namespace Whycespace.Projections.Control.AccessControl.AccessPolicy.Reducer;

public static class AccessPolicyProjectionReducer
{
    public static AccessPolicyReadModel Apply(AccessPolicyReadModel state, AccessPolicyDefinedEventSchema e) =>
        state with
        {
            PolicyId       = e.AggregateId,
            Name           = e.Name,
            Scope          = e.Scope,
            AllowedRoleIds = e.AllowedRoleIds,
            Status         = "Draft"
        };

    public static AccessPolicyReadModel Apply(AccessPolicyReadModel state, AccessPolicyActivatedEventSchema e) =>
        state with { Status = "Active" };

    public static AccessPolicyReadModel Apply(AccessPolicyReadModel state, AccessPolicyRetiredEventSchema e) =>
        state with { Status = "Retired" };
}
