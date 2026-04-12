namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public sealed class CanIssueSpecification
{
    public bool IsSatisfiedBy(InvoiceStatus status)
    {
        return status == InvoiceStatus.Draft;
    }
}
