namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed record SegmentCreatedEvent(
    SegmentId SegmentId,
    SegmentCode Code,
    SegmentName Name,
    SegmentType Type,
    SegmentCriteria Criteria);
