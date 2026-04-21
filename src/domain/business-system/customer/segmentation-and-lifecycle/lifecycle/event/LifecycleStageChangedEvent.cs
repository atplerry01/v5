using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleStageChangedEvent(
    [property: JsonPropertyName("AggregateId")] LifecycleId LifecycleId,
    LifecycleStage From,
    LifecycleStage To,
    DateTimeOffset ChangedAt) : DomainEvent;
