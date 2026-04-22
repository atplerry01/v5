using Whycespace.Shared.Contracts.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Permission;

namespace Whycespace.Projections.Control.AccessControl.Permission.Reducer;

public static class PermissionProjectionReducer
{
    public static PermissionReadModel Apply(PermissionReadModel state, PermissionDefinedEventSchema e) =>
        state with
        {
            PermissionId  = e.AggregateId,
            Name          = e.Name,
            ResourceScope = e.ResourceScope,
            Actions       = e.Actions,
            IsDeprecated  = false
        };

    public static PermissionReadModel Apply(PermissionReadModel state, PermissionDeprecatedEventSchema e) =>
        state with { IsDeprecated = true };
}
