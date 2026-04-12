namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public sealed class CanUseSpecification
{
    public bool IsSatisfiedBy(UsageRightStatus status)
    {
        return status is UsageRightStatus.Available or UsageRightStatus.InUse;
    }
}
