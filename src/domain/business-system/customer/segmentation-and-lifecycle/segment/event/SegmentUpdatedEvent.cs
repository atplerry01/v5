namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed record SegmentUpdatedEvent(
    SegmentId SegmentId,
    SegmentName Name,
    SegmentCriteria Criteria);
