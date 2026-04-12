namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public sealed class CanActivateCapacitySpecification
{
    public bool IsSatisfiedBy(CapacityStatus status)
    {
        return status == CapacityStatus.Pending;
    }
}
