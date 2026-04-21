namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Authority;

public sealed record AuthorityEstablishedEventSchema(
    Guid AggregateId,
    Guid ClusterReference,
    string AuthorityName);

public sealed record AuthorityAttachedEventSchema(
    Guid AggregateId,
    Guid ClusterRef,
    DateTimeOffset EffectiveAt);

public sealed record AuthorityBindingValidatedEventSchema(
    Guid AggregateId,
    Guid Parent,
    string ParentState,
    DateTimeOffset EffectiveAt);

public sealed record AuthorityActivatedEventSchema(
    Guid AggregateId);

public sealed record AuthorityRevokedEventSchema(
    Guid AggregateId);

public sealed record AuthoritySuspendedEventSchema(
    Guid AggregateId);

public sealed record AuthorityReactivatedEventSchema(
    Guid AggregateId);

public sealed record AuthorityRetiredEventSchema(
    Guid AggregateId);
