namespace Whycespace.Domain.BusinessSystem.Billing.BillRun;

public readonly record struct BillRunId
{
    public Guid Value { get; }

    public BillRunId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BillRunId value must not be empty.", nameof(value));

        Value = value;
    }
}
