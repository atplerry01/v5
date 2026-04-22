using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Principal;

namespace Whycespace.Projections.Control.AccessControl.Principal.Reducer;

public static class PrincipalProjectionReducer
{
    public static PrincipalReadModel Apply(PrincipalReadModel state, PrincipalRegisteredEventSchema e) =>
        state with
        {
            PrincipalId = e.AggregateId,
            Name        = e.Name,
            Kind        = e.Kind,
            IdentityId  = e.IdentityId,
            RoleIds     = [],
            Status      = "Active"
        };

    public static PrincipalReadModel Apply(PrincipalReadModel state, PrincipalRoleAssignedEventSchema e) =>
        state with
        {
            RoleIds = [..state.RoleIds, e.RoleId]
        };

    public static PrincipalReadModel Apply(PrincipalReadModel state, PrincipalDeactivatedEventSchema e) =>
        state with { Status = "Deactivated" };
}
