using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Profile;

namespace Whycespace.Projections.Business.Customer.IdentityAndProfile.Profile.Reducer;

public static class ProfileProjectionReducer
{
    public static ProfileReadModel Apply(ProfileReadModel state, ProfileCreatedEventSchema e) =>
        state with
        {
            ProfileId = e.AggregateId,
            CustomerId = e.CustomerId,
            DisplayName = e.DisplayName,
            Status = "Draft",
            Descriptors = new Dictionary<string, string>()
        };

    public static ProfileReadModel Apply(ProfileReadModel state, ProfileRenamedEventSchema e) =>
        state with
        {
            ProfileId = e.AggregateId,
            DisplayName = e.DisplayName
        };

    public static ProfileReadModel Apply(ProfileReadModel state, ProfileDescriptorSetEventSchema e)
    {
        var next = new Dictionary<string, string>(state.Descriptors)
        {
            [e.Key] = e.Value
        };
        return state with
        {
            ProfileId = e.AggregateId,
            Descriptors = next
        };
    }

    public static ProfileReadModel Apply(ProfileReadModel state, ProfileDescriptorRemovedEventSchema e)
    {
        var next = new Dictionary<string, string>(state.Descriptors);
        next.Remove(e.Key);
        return state with
        {
            ProfileId = e.AggregateId,
            Descriptors = next
        };
    }

    public static ProfileReadModel Apply(ProfileReadModel state, ProfileActivatedEventSchema e) =>
        state with
        {
            ProfileId = e.AggregateId,
            Status = "Active"
        };

    public static ProfileReadModel Apply(ProfileReadModel state, ProfileArchivedEventSchema e) =>
        state with
        {
            ProfileId = e.AggregateId,
            Status = "Archived"
        };
}
