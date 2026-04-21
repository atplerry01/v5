using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Lifecycle;

namespace Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Lifecycle.Reducer;

public static class LifecycleProjectionReducer
{
    public static LifecycleReadModel Apply(LifecycleReadModel state, LifecycleStartedEventSchema e) =>
        state with
        {
            LifecycleId = e.AggregateId,
            CustomerId = e.CustomerId,
            Stage = e.InitialStage,
            Status = "Tracking",
            StartedAt = e.StartedAt,
            LastUpdatedAt = e.StartedAt
        };

    public static LifecycleReadModel Apply(LifecycleReadModel state, LifecycleStageChangedEventSchema e) =>
        state with
        {
            LifecycleId = e.AggregateId,
            Stage = e.To,
            LastUpdatedAt = e.ChangedAt
        };

    public static LifecycleReadModel Apply(LifecycleReadModel state, LifecycleClosedEventSchema e) =>
        state with
        {
            LifecycleId = e.AggregateId,
            Status = "Closed",
            ClosedAt = e.ClosedAt,
            LastUpdatedAt = e.ClosedAt
        };
}
