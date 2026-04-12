namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public sealed class CanApplySpecification
{
    public bool IsSatisfiedBy(AllocationStatus status)
    {
        return status == AllocationStatus.Proposed || status == AllocationStatus.Reverted;
    }
}
