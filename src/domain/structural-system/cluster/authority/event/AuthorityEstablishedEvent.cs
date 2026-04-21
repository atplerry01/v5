using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthorityEstablishedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId,
    AuthorityDescriptor Descriptor) : DomainEvent;

public sealed record AuthorityAttachedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;

public sealed record AuthorityActivatedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId) : DomainEvent;

public sealed record AuthorityRevokedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId) : DomainEvent;

public sealed record AuthoritySuspendedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId) : DomainEvent;

public sealed record AuthorityReactivatedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId) : DomainEvent;

public sealed record AuthorityRetiredEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId) : DomainEvent;

public sealed record AuthorityBindingValidatedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId,
    ClusterRef Parent,
    StructuralParentState ParentState,
    DateTimeOffset EffectiveAt) : DomainEvent;
