using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed record SubclusterDefinedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId,
    SubclusterDescriptor Descriptor) : DomainEvent;

public sealed record SubclusterAttachedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;

public sealed record SubclusterActivatedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId) : DomainEvent;

public sealed record SubclusterArchivedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId) : DomainEvent;

public sealed record SubclusterSuspendedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId) : DomainEvent;

public sealed record SubclusterReactivatedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId) : DomainEvent;

public sealed record SubclusterRetiredEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId) : DomainEvent;

public sealed record SubclusterBindingValidatedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId,
    ClusterRef Parent,
    StructuralParentState ParentState,
    DateTimeOffset EffectiveAt) : DomainEvent;
