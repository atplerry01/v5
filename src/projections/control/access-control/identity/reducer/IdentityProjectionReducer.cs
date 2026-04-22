using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Identity;

namespace Whycespace.Projections.Control.AccessControl.Identity.Reducer;

public static class IdentityProjectionReducer
{
    public static IdentityReadModel Apply(IdentityReadModel state, IdentityRegisteredEventSchema e) =>
        state with
        {
            IdentityId = e.AggregateId,
            Name       = e.Name,
            Kind       = e.Kind,
            Status     = "Active"
        };

    public static IdentityReadModel Apply(IdentityReadModel state, IdentitySuspendedEventSchema e) =>
        state with
        {
            Status           = "Suspended",
            SuspensionReason = e.Reason
        };

    public static IdentityReadModel Apply(IdentityReadModel state, IdentityDeactivatedEventSchema e) =>
        state with { Status = "Deactivated" };
}
