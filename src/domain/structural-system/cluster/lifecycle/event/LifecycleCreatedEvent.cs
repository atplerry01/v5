using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed record LifecycleDefinedEvent(
    [property: JsonPropertyName("AggregateId")] LifecycleId LifecycleId,
    LifecycleDescriptor Descriptor) : DomainEvent;

public sealed record LifecycleTransitionedEvent(
    [property: JsonPropertyName("AggregateId")] LifecycleId LifecycleId) : DomainEvent;

public sealed record LifecycleCompletedEvent(
    [property: JsonPropertyName("AggregateId")] LifecycleId LifecycleId) : DomainEvent;
