namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Lifecycle;

public sealed record LifecycleDefinedEventSchema(
    Guid AggregateId,
    Guid ClusterReference,
    string LifecycleName);

public sealed record LifecycleTransitionedEventSchema(
    Guid AggregateId);

public sealed record LifecycleCompletedEventSchema(
    Guid AggregateId);
