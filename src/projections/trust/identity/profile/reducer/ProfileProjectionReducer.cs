using Whycespace.Shared.Contracts.Events.Trust.Identity.Profile;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;

namespace Whycespace.Projections.Trust.Identity.Profile.Reducer;

public static class ProfileProjectionReducer
{
    public static ProfileReadModel Apply(ProfileReadModel state, ProfileCreatedEventSchema e) =>
        state with
        {
            ProfileId = e.AggregateId,
            IdentityReference = e.IdentityReference,
            DisplayName = e.DisplayName,
            ProfileType = e.ProfileType,
            Status = "Inactive",
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        };

    public static ProfileReadModel Apply(ProfileReadModel state, ProfileActivatedEventSchema e) =>
        state with
        {
            ProfileId = e.AggregateId,
            Status = "Active",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static ProfileReadModel Apply(ProfileReadModel state, ProfileDeactivatedEventSchema e) =>
        state with
        {
            ProfileId = e.AggregateId,
            Status = "Inactive",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
}
