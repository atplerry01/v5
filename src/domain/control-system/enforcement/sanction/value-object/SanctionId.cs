namespace Whycespace.Domain.ControlSystem.Enforcement.Sanction;

public readonly record struct SanctionId
{
    public Guid Value { get; }

    public SanctionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SanctionId cannot be empty.", nameof(value));
        Value = value;
    }

    public static SanctionId From(Guid value) => new(value);
}
