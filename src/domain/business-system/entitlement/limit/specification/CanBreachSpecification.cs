namespace Whycespace.Domain.BusinessSystem.Entitlement.Limit;

public sealed class CanBreachSpecification
{
    public bool IsSatisfiedBy(LimitStatus status)
    {
        return status == LimitStatus.Enforced;
    }
}
