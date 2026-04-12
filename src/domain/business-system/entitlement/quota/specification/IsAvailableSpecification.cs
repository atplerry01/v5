namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed class IsAvailableSpecification
{
    public bool IsSatisfiedBy(QuotaStatus status)
    {
        return status == QuotaStatus.Available;
    }
}
