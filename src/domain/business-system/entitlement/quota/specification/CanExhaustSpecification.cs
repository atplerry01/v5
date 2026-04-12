namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed class CanExhaustSpecification
{
    public bool IsSatisfiedBy(QuotaStatus status)
    {
        return status == QuotaStatus.Consumed;
    }
}
