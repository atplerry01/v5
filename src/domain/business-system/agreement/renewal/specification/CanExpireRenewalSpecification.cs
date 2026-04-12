namespace Whycespace.Domain.BusinessSystem.Agreement.Renewal;

public sealed class CanExpireRenewalSpecification
{
    public bool IsSatisfiedBy(RenewalStatus status)
    {
        return status == RenewalStatus.Pending;
    }
}
