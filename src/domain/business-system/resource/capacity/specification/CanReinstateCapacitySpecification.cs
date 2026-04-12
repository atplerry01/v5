namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public sealed class CanReinstateCapacitySpecification
{
    public bool IsSatisfiedBy(CapacityStatus status)
    {
        return status == CapacityStatus.Suspended;
    }
}
