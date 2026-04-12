namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public readonly record struct UsageRightReferenceId
{
    public Guid Value { get; }

    public UsageRightReferenceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UsageRightReferenceId value must not be empty.", nameof(value));
        Value = value;
    }
}
