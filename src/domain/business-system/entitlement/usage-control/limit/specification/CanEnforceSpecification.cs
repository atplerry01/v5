namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public sealed class CanEnforceSpecification
{
    public bool IsSatisfiedBy(LimitStatus status)
    {
        return status == LimitStatus.Defined;
    }
}
