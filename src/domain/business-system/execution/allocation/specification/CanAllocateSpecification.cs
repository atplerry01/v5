namespace Whycespace.Domain.BusinessSystem.Execution.Allocation;

public sealed class CanAllocateSpecification
{
    public bool IsSatisfiedBy(AllocationStatus status)
    {
        return status == AllocationStatus.Pending;
    }
}
