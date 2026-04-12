namespace Whycespace.Domain.BusinessSystem.Entitlement.Limit;

public readonly record struct LimitSubjectId
{
    public Guid Value { get; }

    public LimitSubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LimitSubjectId value must not be empty.", nameof(value));
        Value = value;
    }
}
