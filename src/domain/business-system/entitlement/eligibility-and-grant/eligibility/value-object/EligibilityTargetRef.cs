namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct EligibilityTargetRef
{
    public Guid Value { get; }

    public EligibilityTargetRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EligibilityTargetRef value must not be empty.", nameof(value));

        Value = value;
    }
}
