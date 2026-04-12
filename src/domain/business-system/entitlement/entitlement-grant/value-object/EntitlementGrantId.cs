namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public readonly record struct EntitlementGrantId
{
    public Guid Value { get; }

    public EntitlementGrantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EntitlementGrantId value must not be empty.", nameof(value));
        Value = value;
    }
}
