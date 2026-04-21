namespace Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Segment;

public sealed record SegmentCreatedEventSchema(
    Guid AggregateId,
    string Code,
    string Name,
    string Type,
    string Criteria);

public sealed record SegmentUpdatedEventSchema(
    Guid AggregateId,
    string Name,
    string Criteria);

public sealed record SegmentActivatedEventSchema(Guid AggregateId);

public sealed record SegmentArchivedEventSchema(Guid AggregateId);
