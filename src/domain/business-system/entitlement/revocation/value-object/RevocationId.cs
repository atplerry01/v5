namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public readonly record struct RevocationId
{
    public Guid Value { get; }

    public RevocationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RevocationId value must not be empty.", nameof(value));
        Value = value;
    }
}
