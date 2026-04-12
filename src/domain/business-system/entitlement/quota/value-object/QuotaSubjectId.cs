namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public readonly record struct QuotaSubjectId
{
    public Guid Value { get; }

    public QuotaSubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("QuotaSubjectId value must not be empty.", nameof(value));
        Value = value;
    }
}
