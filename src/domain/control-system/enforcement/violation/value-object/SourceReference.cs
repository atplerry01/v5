namespace Whycespace.Domain.ControlSystem.Enforcement.Violation;

public readonly record struct SourceReference
{
    public Guid Value { get; }

    public SourceReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SourceReference cannot be empty.", nameof(value));
        Value = value;
    }

    public static SourceReference From(Guid value) => new(value);
}
