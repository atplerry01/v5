namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public readonly record struct UsageRightId
{
    public Guid Value { get; }

    public UsageRightId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UsageRightId value must not be empty.", nameof(value));
        Value = value;
    }
}
