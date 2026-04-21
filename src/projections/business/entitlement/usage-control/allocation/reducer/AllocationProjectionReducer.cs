using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Allocation;

namespace Whycespace.Projections.Business.Entitlement.UsageControl.Allocation.Reducer;

public static class AllocationProjectionReducer
{
    public static AllocationReadModel Apply(AllocationReadModel state, AllocationCreatedEventSchema e) =>
        state with
        {
            AllocationId = e.AggregateId,
            ResourceId = e.ResourceId,
            RequestedCapacity = e.RequestedCapacity,
            Status = "Pending",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static AllocationReadModel Apply(AllocationReadModel state, AllocationAllocatedEventSchema e) =>
        state with
        {
            AllocationId = e.AggregateId,
            Status = "Allocated"
        };

    public static AllocationReadModel Apply(AllocationReadModel state, AllocationReleasedEventSchema e) =>
        state with
        {
            AllocationId = e.AggregateId,
            Status = "Released"
        };
}
