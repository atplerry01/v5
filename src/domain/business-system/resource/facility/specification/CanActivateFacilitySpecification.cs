namespace Whycespace.Domain.BusinessSystem.Resource.Facility;

public sealed class CanActivateFacilitySpecification
{
    public bool IsSatisfiedBy(FacilityStatus status)
    {
        return status == FacilityStatus.Pending;
    }
}
