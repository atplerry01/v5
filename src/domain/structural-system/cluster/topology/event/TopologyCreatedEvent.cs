using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed record TopologyDefinedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyId TopologyId,
    TopologyDescriptor Descriptor) : DomainEvent;

public sealed record TopologyValidatedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyId TopologyId) : DomainEvent;

public sealed record TopologyLockedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyId TopologyId) : DomainEvent;
