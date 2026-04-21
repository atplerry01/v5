using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderTier;

namespace Whycespace.Projections.Business.Provider.ProviderCore.ProviderTier.Reducer;

public static class ProviderTierProjectionReducer
{
    public static ProviderTierReadModel Apply(ProviderTierReadModel state, ProviderTierCreatedEventSchema e) =>
        state with
        {
            ProviderTierId = e.AggregateId,
            Code = e.Code,
            Name = e.Name,
            Rank = e.Rank,
            Status = "Draft"
        };

    public static ProviderTierReadModel Apply(ProviderTierReadModel state, ProviderTierUpdatedEventSchema e) =>
        state with
        {
            ProviderTierId = e.AggregateId,
            Name = e.Name,
            Rank = e.Rank
        };

    public static ProviderTierReadModel Apply(ProviderTierReadModel state, ProviderTierActivatedEventSchema e) =>
        state with
        {
            ProviderTierId = e.AggregateId,
            Status = "Active"
        };

    public static ProviderTierReadModel Apply(ProviderTierReadModel state, ProviderTierArchivedEventSchema e) =>
        state with
        {
            ProviderTierId = e.AggregateId,
            Status = "Archived"
        };
}
