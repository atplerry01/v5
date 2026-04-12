namespace Whycespace.Domain.BusinessSystem.Agreement.Renewal;

public sealed class CanRenewSpecification
{
    public bool IsSatisfiedBy(RenewalStatus status)
    {
        return status == RenewalStatus.Pending;
    }
}
