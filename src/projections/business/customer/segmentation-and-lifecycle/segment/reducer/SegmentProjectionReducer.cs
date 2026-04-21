using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Segment;

namespace Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Segment.Reducer;

public static class SegmentProjectionReducer
{
    public static SegmentReadModel Apply(SegmentReadModel state, SegmentCreatedEventSchema e) =>
        state with
        {
            SegmentId = e.AggregateId,
            Code = e.Code,
            Name = e.Name,
            Type = e.Type,
            Criteria = e.Criteria,
            Status = "Draft"
        };

    public static SegmentReadModel Apply(SegmentReadModel state, SegmentUpdatedEventSchema e) =>
        state with
        {
            SegmentId = e.AggregateId,
            Name = e.Name,
            Criteria = e.Criteria
        };

    public static SegmentReadModel Apply(SegmentReadModel state, SegmentActivatedEventSchema e) =>
        state with
        {
            SegmentId = e.AggregateId,
            Status = "Active"
        };

    public static SegmentReadModel Apply(SegmentReadModel state, SegmentArchivedEventSchema e) =>
        state with
        {
            SegmentId = e.AggregateId,
            Status = "Archived"
        };
}
