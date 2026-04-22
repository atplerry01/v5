using Whycespace.Shared.Contracts.Events.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;

namespace Whycespace.Projections.Trust.Identity.Registry.Reducer;

public static class RegistryProjectionReducer
{
    public static RegistryReadModel Apply(RegistryReadModel state, RegistrationInitiatedEventSchema e) =>
        state with
        {
            RegistryId = e.AggregateId,
            Email = e.Email,
            RegistrationType = e.RegistrationType,
            Status = "Initiated",
            InitiatedAt = e.InitiatedAt,
            LastUpdatedAt = e.InitiatedAt
        };

    public static RegistryReadModel Apply(RegistryReadModel state, RegistrationVerifiedEventSchema e) =>
        state with
        {
            RegistryId = e.AggregateId,
            Status = "Verified",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static RegistryReadModel Apply(RegistryReadModel state, RegistrationActivatedEventSchema e) =>
        state with
        {
            RegistryId = e.AggregateId,
            Status = "Activated",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static RegistryReadModel Apply(RegistryReadModel state, RegistrationRejectedEventSchema e) =>
        state with
        {
            RegistryId = e.AggregateId,
            Status = "Rejected",
            RejectionReason = e.Reason,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static RegistryReadModel Apply(RegistryReadModel state, RegistrationLockedEventSchema e) =>
        state with
        {
            RegistryId = e.AggregateId,
            Status = "Locked",
            IsLocked = true,
            LockReason = e.Reason,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
}
