namespace Whycespace.Domain.BusinessSystem.Billing.Receivable;

public sealed class CanSettleSpecification
{
    public bool IsSatisfiedBy(ReceivableStatus status)
    {
        return status == ReceivableStatus.Outstanding;
    }
}
