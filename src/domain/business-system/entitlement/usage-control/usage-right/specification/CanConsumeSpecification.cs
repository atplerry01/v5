namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed class CanConsumeSpecification
{
    public bool IsSatisfiedBy(UsageRightStatus status)
    {
        return status == UsageRightStatus.InUse;
    }
}
