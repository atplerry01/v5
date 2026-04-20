namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public sealed class CanExpireRenewalSpecification
{
    public bool IsSatisfiedBy(RenewalStatus status)
    {
        return status == RenewalStatus.Pending;
    }
}
