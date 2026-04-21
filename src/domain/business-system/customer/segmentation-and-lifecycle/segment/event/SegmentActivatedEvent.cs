using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed record SegmentActivatedEvent(
    [property: JsonPropertyName("AggregateId")] SegmentId SegmentId) : DomainEvent;
