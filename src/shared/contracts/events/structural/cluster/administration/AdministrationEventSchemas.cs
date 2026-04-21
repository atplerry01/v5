namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Administration;

public sealed record AdministrationEstablishedEventSchema(
    Guid AggregateId,
    Guid ClusterReference,
    string AdministrationName);

public sealed record AdministrationAttachedEventSchema(
    Guid AggregateId,
    Guid ClusterRef,
    DateTimeOffset EffectiveAt);

public sealed record AdministrationBindingValidatedEventSchema(
    Guid AggregateId,
    Guid Parent,
    string ParentState,
    DateTimeOffset EffectiveAt);

public sealed record AdministrationActivatedEventSchema(
    Guid AggregateId);

public sealed record AdministrationSuspendedEventSchema(
    Guid AggregateId);

public sealed record AdministrationRetiredEventSchema(
    Guid AggregateId);
