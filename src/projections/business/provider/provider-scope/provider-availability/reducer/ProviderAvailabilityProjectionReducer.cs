using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderAvailability;

namespace Whycespace.Projections.Business.Provider.ProviderScope.ProviderAvailability.Reducer;

public static class ProviderAvailabilityProjectionReducer
{
    public static ProviderAvailabilityReadModel Apply(ProviderAvailabilityReadModel state, ProviderAvailabilityCreatedEventSchema e) =>
        state with
        {
            ProviderAvailabilityId = e.AggregateId,
            ProviderId = e.ProviderId,
            StartsAt = e.StartsAt,
            EndsAt = e.EndsAt,
            Status = "Draft"
        };

    public static ProviderAvailabilityReadModel Apply(ProviderAvailabilityReadModel state, ProviderAvailabilityUpdatedEventSchema e) =>
        state with
        {
            ProviderAvailabilityId = e.AggregateId,
            StartsAt = e.StartsAt,
            EndsAt = e.EndsAt
        };

    public static ProviderAvailabilityReadModel Apply(ProviderAvailabilityReadModel state, ProviderAvailabilityActivatedEventSchema e) =>
        state with
        {
            ProviderAvailabilityId = e.AggregateId,
            Status = "Active"
        };

    public static ProviderAvailabilityReadModel Apply(ProviderAvailabilityReadModel state, ProviderAvailabilityArchivedEventSchema e) =>
        state with
        {
            ProviderAvailabilityId = e.AggregateId,
            Status = "Archived"
        };
}
