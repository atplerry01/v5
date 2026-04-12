namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public sealed class CanViolateSpecification
{
    public bool IsSatisfiedBy(RestrictionStatus status)
    {
        return status == RestrictionStatus.Active;
    }
}
