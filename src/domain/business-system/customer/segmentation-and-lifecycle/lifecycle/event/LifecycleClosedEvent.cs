using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleClosedEvent(
    [property: JsonPropertyName("AggregateId")] LifecycleId LifecycleId,
    DateTimeOffset ClosedAt) : DomainEvent;
