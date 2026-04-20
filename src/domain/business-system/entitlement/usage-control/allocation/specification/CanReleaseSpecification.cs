namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;

public sealed class CanReleaseSpecification
{
    public bool IsSatisfiedBy(AllocationStatus status)
    {
        return status == AllocationStatus.Allocated;
    }
}
