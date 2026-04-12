namespace Whycespace.Domain.BusinessSystem.Billing.Receivable;

public readonly record struct ReceivableId
{
    public Guid Value { get; }

    public ReceivableId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReceivableId value must not be empty.", nameof(value));

        Value = value;
    }
}
