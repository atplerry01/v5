namespace Whycespace.Domain.BusinessSystem.Execution.Allocation;

public sealed class IsAllocatedSpecification
{
    public bool IsSatisfiedBy(AllocationStatus status)
    {
        return status == AllocationStatus.Allocated;
    }
}
