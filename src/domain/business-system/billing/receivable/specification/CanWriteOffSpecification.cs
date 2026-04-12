namespace Whycespace.Domain.BusinessSystem.Billing.Receivable;

public sealed class CanWriteOffSpecification
{
    public bool IsSatisfiedBy(ReceivableStatus status)
    {
        return status == ReceivableStatus.Outstanding;
    }
}
