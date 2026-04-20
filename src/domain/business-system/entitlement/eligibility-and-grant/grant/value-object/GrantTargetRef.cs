namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantTargetRef
{
    public Guid Value { get; }

    public GrantTargetRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GrantTargetRef value must not be empty.", nameof(value));

        Value = value;
    }
}
