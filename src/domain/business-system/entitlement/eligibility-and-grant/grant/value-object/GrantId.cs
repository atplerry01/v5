namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantId
{
    public Guid Value { get; }

    public GrantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GrantId value must not be empty.", nameof(value));

        Value = value;
    }
}
