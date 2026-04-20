namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct EligibilitySubjectRef
{
    public Guid Value { get; }

    public EligibilitySubjectRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EligibilitySubjectRef value must not be empty.", nameof(value));

        Value = value;
    }
}
