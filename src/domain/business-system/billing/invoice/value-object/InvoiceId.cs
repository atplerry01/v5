namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public readonly record struct InvoiceId
{
    public Guid Value { get; }

    public InvoiceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("InvoiceId value must not be empty.", nameof(value));

        Value = value;
    }
}
