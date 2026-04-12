namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public readonly record struct EntitlementRightId
{
    public Guid Value { get; }

    public EntitlementRightId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EntitlementRightId value must not be empty.", nameof(value));
        Value = value;
    }
}
