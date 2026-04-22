using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed record LifecycleCompletedEvent(
    [property: JsonPropertyName("AggregateId")] LifecycleId LifecycleId) : DomainEvent;
