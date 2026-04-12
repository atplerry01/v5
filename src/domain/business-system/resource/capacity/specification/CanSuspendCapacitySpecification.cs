namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public sealed class CanSuspendCapacitySpecification
{
    public bool IsSatisfiedBy(CapacityStatus status)
    {
        return status == CapacityStatus.Active;
    }
}
