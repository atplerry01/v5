namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public sealed class CanRevertSpecification
{
    public bool IsSatisfiedBy(AllocationStatus status)
    {
        return status == AllocationStatus.Applied;
    }
}
