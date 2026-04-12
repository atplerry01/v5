namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public readonly record struct EligibilityId
{
    public Guid Value { get; }

    public EligibilityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EligibilityId value must not be empty.", nameof(value));

        Value = value;
    }
}
