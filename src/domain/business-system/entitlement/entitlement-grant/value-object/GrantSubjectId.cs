namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public readonly record struct GrantSubjectId
{
    public Guid Value { get; }

    public GrantSubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GrantSubjectId value must not be empty.", nameof(value));
        Value = value;
    }
}
