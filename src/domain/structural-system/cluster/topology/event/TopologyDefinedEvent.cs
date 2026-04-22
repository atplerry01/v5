using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed record TopologyDefinedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyId TopologyId,
    TopologyDescriptor Descriptor) : DomainEvent;
