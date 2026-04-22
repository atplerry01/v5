using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthorityAttachedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;
