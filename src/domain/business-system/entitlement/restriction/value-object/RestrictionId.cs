namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public readonly record struct RestrictionId
{
    public Guid Value { get; }

    public RestrictionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RestrictionId value must not be empty.", nameof(value));

        Value = value;
    }
}