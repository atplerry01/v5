using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvAttachedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;
