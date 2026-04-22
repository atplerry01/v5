using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Role;

namespace Whycespace.Projections.Control.AccessControl.Role.Reducer;

public static class RoleProjectionReducer
{
    public static RoleReadModel Apply(RoleReadModel state, RoleDefinedEventSchema e) =>
        state with
        {
            RoleId        = e.AggregateId,
            Name          = e.Name,
            PermissionIds = e.PermissionIds,
            ParentRoleId  = e.ParentRoleId,
            IsDeprecated  = false
        };

    public static RoleReadModel Apply(RoleReadModel state, RolePermissionAddedEventSchema e) =>
        state with
        {
            PermissionIds = [..state.PermissionIds, e.PermissionId]
        };

    public static RoleReadModel Apply(RoleReadModel state, RoleDeprecatedEventSchema e) =>
        state with { IsDeprecated = true };
}
