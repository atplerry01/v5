namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

public readonly record struct RestrictionId
{
    public Guid Value { get; }

    public RestrictionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RestrictionId cannot be empty.", nameof(value));
        Value = value;
    }

    public static RestrictionId From(Guid value) => new(value);
}
