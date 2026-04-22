using Whycespace.Shared.Contracts.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Authorization;

namespace Whycespace.Projections.Control.AccessControl.Authorization.Reducer;

public static class AuthorizationProjectionReducer
{
    public static AuthorizationReadModel Apply(AuthorizationReadModel state, AuthorizationGrantedEventSchema e) =>
        state with
        {
            AuthorizationId = e.AggregateId,
            SubjectId       = e.SubjectId,
            RoleIds         = e.RoleIds,
            ValidFrom       = e.ValidFrom,
            ValidTo         = e.ValidTo,
            IsRevoked       = false
        };

    public static AuthorizationReadModel Apply(AuthorizationReadModel state, AuthorizationRevokedEventSchema e) =>
        state with { IsRevoked = true };
}
