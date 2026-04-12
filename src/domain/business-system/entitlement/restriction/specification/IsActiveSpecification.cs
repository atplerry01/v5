namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(RestrictionStatus status)
    {
        return status == RestrictionStatus.Active;
    }
}
