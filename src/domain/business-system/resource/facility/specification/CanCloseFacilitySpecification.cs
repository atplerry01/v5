namespace Whycespace.Domain.BusinessSystem.Resource.Facility;

public sealed class CanCloseFacilitySpecification
{
    public bool IsSatisfiedBy(FacilityStatus status)
    {
        return status == FacilityStatus.Active;
    }
}
