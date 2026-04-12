namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed record TopologyDefinedEvent(TopologyId TopologyId, TopologyDescriptor Descriptor);

public sealed record TopologyValidatedEvent(TopologyId TopologyId);

public sealed record TopologyLockedEvent(TopologyId TopologyId);
