namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public sealed class CanLiftSpecification
{
    public bool IsSatisfiedBy(RestrictionStatus status)
    {
        return status == RestrictionStatus.Violated;
    }
}
