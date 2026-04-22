using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed record ClusterAuthorityBoundEvent(
    [property: JsonPropertyName("AggregateId")] ClusterId ClusterId,
    ClusterAuthorityRef Authority) : DomainEvent;
