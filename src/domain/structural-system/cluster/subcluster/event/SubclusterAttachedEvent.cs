using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed record SubclusterAttachedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;
