namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public sealed class PaymentAllocation
{
    public Guid AllocationId { get; }
    public Guid InvoiceLineId { get; }
    public decimal Amount { get; }

    public PaymentAllocation(Guid allocationId, Guid invoiceLineId, decimal amount)
    {
        if (allocationId == Guid.Empty)
            throw new ArgumentException("AllocationId must not be empty.", nameof(allocationId));

        if (invoiceLineId == Guid.Empty)
            throw new ArgumentException("InvoiceLineId must not be empty.", nameof(invoiceLineId));

        if (amount <= 0)
            throw new ArgumentException("Allocation amount must be greater than zero.", nameof(amount));

        AllocationId = allocationId;
        InvoiceLineId = invoiceLineId;
        Amount = amount;
    }
}
