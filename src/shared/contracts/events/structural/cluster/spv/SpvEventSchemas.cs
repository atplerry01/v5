namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Spv;

public sealed record SpvCreatedEventSchema(
    Guid AggregateId,
    Guid ClusterReference,
    string SpvName,
    string SpvType);

public sealed record SpvAttachedEventSchema(
    Guid AggregateId,
    Guid ClusterRef,
    DateTimeOffset EffectiveAt);

public sealed record SpvBindingValidatedEventSchema(
    Guid AggregateId,
    Guid Parent,
    string ParentState,
    DateTimeOffset EffectiveAt);

public sealed record SpvActivatedEventSchema(
    Guid AggregateId);

public sealed record SpvSuspendedEventSchema(
    Guid AggregateId);

public sealed record SpvClosedEventSchema(
    Guid AggregateId);

public sealed record SpvReactivatedEventSchema(
    Guid AggregateId);

public sealed record SpvRetiredEventSchema(
    Guid AggregateId);
