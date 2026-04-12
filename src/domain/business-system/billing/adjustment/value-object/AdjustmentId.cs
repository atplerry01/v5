namespace Whycespace.Domain.BusinessSystem.Billing.Adjustment;

public readonly record struct AdjustmentId
{
    public Guid Value { get; }

    public AdjustmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AdjustmentId value must not be empty.", nameof(value));

        Value = value;
    }
}
