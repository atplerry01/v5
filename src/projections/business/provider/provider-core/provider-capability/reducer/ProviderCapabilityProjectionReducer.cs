using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderCapability;

namespace Whycespace.Projections.Business.Provider.ProviderCore.ProviderCapability.Reducer;

public static class ProviderCapabilityProjectionReducer
{
    public static ProviderCapabilityReadModel Apply(ProviderCapabilityReadModel state, ProviderCapabilityCreatedEventSchema e) =>
        state with
        {
            ProviderCapabilityId = e.AggregateId,
            ProviderId = e.ProviderId,
            Code = e.Code,
            Name = e.Name,
            Status = "Draft"
        };

    public static ProviderCapabilityReadModel Apply(ProviderCapabilityReadModel state, ProviderCapabilityUpdatedEventSchema e) =>
        state with
        {
            ProviderCapabilityId = e.AggregateId,
            Name = e.Name
        };

    public static ProviderCapabilityReadModel Apply(ProviderCapabilityReadModel state, ProviderCapabilityActivatedEventSchema e) =>
        state with
        {
            ProviderCapabilityId = e.AggregateId,
            Status = "Active"
        };

    public static ProviderCapabilityReadModel Apply(ProviderCapabilityReadModel state, ProviderCapabilityArchivedEventSchema e) =>
        state with
        {
            ProviderCapabilityId = e.AggregateId,
            Status = "Archived"
        };
}
