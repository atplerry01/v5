namespace Whycespace.Domain.BusinessSystem.Billing.BillRun;

public sealed class BillRunItem
{
    public Guid InvoiceReference { get; }
    public string Label { get; }

    public BillRunItem(Guid invoiceReference, string label)
    {
        if (invoiceReference == Guid.Empty)
            throw new ArgumentException("InvoiceReference must not be empty.", nameof(invoiceReference));

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label must not be empty.", nameof(label));

        InvoiceReference = invoiceReference;
        Label = label;
    }
}
