namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Provider;

public sealed record ProviderRegisteredEventSchema(
    Guid AggregateId,
    Guid ClusterReference,
    string ProviderName);

public sealed record ProviderAttachedEventSchema(
    Guid AggregateId,
    Guid ClusterRef,
    DateTimeOffset EffectiveAt);

public sealed record ProviderBindingValidatedEventSchema(
    Guid AggregateId,
    Guid Parent,
    string ParentState,
    DateTimeOffset EffectiveAt);

public sealed record ProviderActivatedEventSchema(
    Guid AggregateId);

public sealed record ProviderSuspendedEventSchema(
    Guid AggregateId);

public sealed record ProviderReactivatedEventSchema(
    Guid AggregateId);

public sealed record ProviderRetiredEventSchema(
    Guid AggregateId);
