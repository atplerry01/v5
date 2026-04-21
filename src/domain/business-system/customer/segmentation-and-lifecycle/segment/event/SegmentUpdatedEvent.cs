using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed record SegmentUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] SegmentId SegmentId,
    SegmentName Name,
    SegmentCriteria Criteria) : DomainEvent;
