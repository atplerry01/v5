namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed class CanConsumeSpecification
{
    public bool IsSatisfiedBy(QuotaStatus status)
    {
        return status == QuotaStatus.Available || status == QuotaStatus.Consumed;
    }
}
