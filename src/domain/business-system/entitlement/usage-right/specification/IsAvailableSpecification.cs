namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public sealed class IsAvailableSpecification
{
    public bool IsSatisfiedBy(UsageRightStatus status)
    {
        return status == UsageRightStatus.Available;
    }
}
