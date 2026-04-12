namespace Whycespace.Domain.BusinessSystem.Execution.Allocation;

public sealed class CanReleaseSpecification
{
    public bool IsSatisfiedBy(AllocationStatus status)
    {
        return status == AllocationStatus.Allocated;
    }
}
