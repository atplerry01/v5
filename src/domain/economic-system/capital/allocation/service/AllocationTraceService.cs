namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed class AllocationTraceService
{
    public bool ValidateTraceability(CapitalAllocationAggregate allocation) =>
        allocation.SourceAccountId != Guid.Empty &&
        allocation.TargetId.Value != Guid.Empty;
}
