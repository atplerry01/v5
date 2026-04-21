namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Topology;

public sealed record TopologyDefinedEventSchema(
    Guid AggregateId,
    Guid ClusterReference,
    string TopologyName);

public sealed record TopologyValidatedEventSchema(
    Guid AggregateId);

public sealed record TopologyLockedEventSchema(
    Guid AggregateId);
