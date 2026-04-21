using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleStartedEvent(
    [property: JsonPropertyName("AggregateId")] LifecycleId LifecycleId,
    CustomerRef Customer,
    LifecycleStage InitialStage,
    DateTimeOffset StartedAt) : DomainEvent;
