namespace Whycespace.Domain.BusinessSystem.Entitlement.Allocation;

public sealed class CanAllocateSpecification
{
    public bool IsSatisfiedBy(AllocationStatus status)
    {
        return status == AllocationStatus.Pending;
    }
}
