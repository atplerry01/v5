namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public readonly record struct RevocationTargetId
{
    public Guid Value { get; }

    public RevocationTargetId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RevocationTargetId value must not be empty.", nameof(value));
        Value = value;
    }
}
