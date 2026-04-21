namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Subcluster;

public sealed record SubclusterDefinedEventSchema(
    Guid AggregateId,
    Guid ParentClusterReference,
    string SubclusterName);

public sealed record SubclusterAttachedEventSchema(
    Guid AggregateId,
    Guid ClusterRef,
    DateTimeOffset EffectiveAt);

public sealed record SubclusterBindingValidatedEventSchema(
    Guid AggregateId,
    Guid Parent,
    string ParentState,
    DateTimeOffset EffectiveAt);

public sealed record SubclusterActivatedEventSchema(
    Guid AggregateId);

public sealed record SubclusterSuspendedEventSchema(
    Guid AggregateId);

public sealed record SubclusterReactivatedEventSchema(
    Guid AggregateId);

public sealed record SubclusterArchivedEventSchema(
    Guid AggregateId);

public sealed record SubclusterRetiredEventSchema(
    Guid AggregateId);
