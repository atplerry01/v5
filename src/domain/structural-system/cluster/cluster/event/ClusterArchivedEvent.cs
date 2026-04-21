using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed record ClusterArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ClusterId ClusterId) : DomainEvent;
