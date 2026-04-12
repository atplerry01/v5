namespace Whycespace.Domain.BusinessSystem.Entitlement.Limit;

public sealed class IsEnforcedSpecification
{
    public bool IsSatisfiedBy(LimitStatus status)
    {
        return status == LimitStatus.Enforced;
    }
}
