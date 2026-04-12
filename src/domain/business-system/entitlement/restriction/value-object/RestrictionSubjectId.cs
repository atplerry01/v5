namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public readonly record struct RestrictionSubjectId
{
    public Guid Value { get; }

    public RestrictionSubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RestrictionSubjectId value must not be empty.", nameof(value));
        Value = value;
    }
}
