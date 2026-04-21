using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvCreatedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId,
    SpvDescriptor Descriptor) : DomainEvent;

public sealed record SpvAttachedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;

public sealed record SpvActivatedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId) : DomainEvent;

public sealed record SpvSuspendedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId) : DomainEvent;

public sealed record SpvClosedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId) : DomainEvent;

public sealed record SpvReactivatedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId) : DomainEvent;

public sealed record SpvRetiredEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId) : DomainEvent;

public sealed record SpvBindingValidatedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId,
    ClusterRef Parent,
    StructuralParentState ParentState,
    DateTimeOffset EffectiveAt) : DomainEvent;
