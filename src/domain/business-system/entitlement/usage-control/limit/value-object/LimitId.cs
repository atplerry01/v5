namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public readonly record struct LimitId
{
    public Guid Value { get; }

    public LimitId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LimitId value must not be empty.", nameof(value));

        Value = value;
    }
}