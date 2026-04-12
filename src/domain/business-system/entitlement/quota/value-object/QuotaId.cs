namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public readonly record struct QuotaId
{
    public Guid Value { get; }

    public QuotaId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("QuotaId value must not be empty.", nameof(value));

        Value = value;
    }
}