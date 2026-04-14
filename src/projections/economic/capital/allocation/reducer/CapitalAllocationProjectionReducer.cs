using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Allocation;

namespace Whycespace.Projections.Economic.Capital.Allocation.Reducer;

public static class CapitalAllocationProjectionReducer
{
    public static CapitalAllocationReadModel Apply(CapitalAllocationReadModel state, AllocationCreatedEventSchema e) =>
        state with
        {
            AllocationId = e.AggregateId,
            SourceAccountId = e.SourceAccountId,
            TargetId = e.TargetId,
            Amount = e.Amount,
            Currency = e.Currency,
            Status = "Pending",
            AllocatedAt = e.AllocatedAt
        };

    public static CapitalAllocationReadModel Apply(CapitalAllocationReadModel state, AllocationReleasedEventSchema e) =>
        state with
        {
            AllocationId = e.AggregateId,
            Status = "Released"
        };

    public static CapitalAllocationReadModel Apply(CapitalAllocationReadModel state, AllocationCompletedEventSchema e) =>
        state with
        {
            AllocationId = e.AggregateId,
            Status = "Completed"
        };

    public static CapitalAllocationReadModel Apply(CapitalAllocationReadModel state, CapitalAllocatedToSpvEventSchema e) =>
        state with
        {
            AllocationId = e.AggregateId,
            TargetType = "SPV",
            SpvTargetId = e.SpvTargetId,
            OwnershipPercentage = e.OwnershipPercentage
        };
}
