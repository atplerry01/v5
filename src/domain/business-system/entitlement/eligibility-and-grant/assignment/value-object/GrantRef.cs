namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct GrantRef
{
    public Guid Value { get; }

    public GrantRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GrantRef value must not be empty.", nameof(value));

        Value = value;
    }
}
