namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public readonly record struct SubjectId
{
    public Guid Value { get; }

    public SubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubjectId value must not be empty.", nameof(value));
        Value = value;
    }
}
