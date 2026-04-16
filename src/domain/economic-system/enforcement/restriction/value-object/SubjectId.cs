namespace Whycespace.Domain.EconomicSystem.Enforcement.Restriction;

public readonly record struct SubjectId
{
    public Guid Value { get; }

    public SubjectId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubjectId cannot be empty.", nameof(value));
        Value = value;
    }

    public static SubjectId From(Guid value) => new(value);
}
