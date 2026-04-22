using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed record TopologyLockedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyId TopologyId) : DomainEvent;
