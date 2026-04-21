using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public sealed record ProviderAttachedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderId ProviderId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;
