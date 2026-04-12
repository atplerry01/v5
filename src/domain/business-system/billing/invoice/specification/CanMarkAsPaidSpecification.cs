namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public sealed class CanMarkAsPaidSpecification
{
    public bool IsSatisfiedBy(InvoiceStatus status)
    {
        return status == InvoiceStatus.Issued;
    }
}
