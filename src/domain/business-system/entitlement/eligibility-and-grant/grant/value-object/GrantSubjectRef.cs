namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantSubjectRef
{
    public Guid Value { get; }

    public GrantSubjectRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GrantSubjectRef value must not be empty.", nameof(value));

        Value = value;
    }
}
