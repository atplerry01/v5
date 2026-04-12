namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public sealed class CanCancelSpecification
{
    public bool IsSatisfiedBy(InvoiceStatus status)
    {
        return status == InvoiceStatus.Draft || status == InvoiceStatus.Issued;
    }
}
