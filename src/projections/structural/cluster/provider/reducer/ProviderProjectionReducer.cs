using Whycespace.Shared.Contracts.Events.Structural.Cluster.Provider;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;

namespace Whycespace.Projections.Structural.Cluster.Provider.Reducer;

public static class ProviderProjectionReducer
{
    public static ProviderReadModel Apply(ProviderReadModel state, ProviderRegisteredEventSchema e, DateTimeOffset at) =>
        state with
        {
            ProviderId = e.AggregateId,
            ClusterReference = e.ClusterReference,
            ProviderName = e.ProviderName,
            Status = "Registered",
            LastModifiedAt = at
        };

    public static ProviderReadModel Apply(ProviderReadModel state, ProviderAttachedEventSchema e, DateTimeOffset at) =>
        state with
        {
            ProviderId = e.AggregateId,
            AttachedClusterRef = e.ClusterRef,
            AttachedAt = e.EffectiveAt,
            LastModifiedAt = at
        };

    public static ProviderReadModel Apply(ProviderReadModel state, ProviderBindingValidatedEventSchema e, DateTimeOffset at) =>
        state with { ProviderId = e.AggregateId, LastModifiedAt = at };

    public static ProviderReadModel Apply(ProviderReadModel state, ProviderActivatedEventSchema e, DateTimeOffset at) =>
        state with { ProviderId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static ProviderReadModel Apply(ProviderReadModel state, ProviderSuspendedEventSchema e, DateTimeOffset at) =>
        state with { ProviderId = e.AggregateId, Status = "Suspended", LastModifiedAt = at };

    public static ProviderReadModel Apply(ProviderReadModel state, ProviderReactivatedEventSchema e, DateTimeOffset at) =>
        state with { ProviderId = e.AggregateId, Status = "Active", LastModifiedAt = at };

    public static ProviderReadModel Apply(ProviderReadModel state, ProviderRetiredEventSchema e, DateTimeOffset at) =>
        state with { ProviderId = e.AggregateId, Status = "Retired", LastModifiedAt = at };
}
