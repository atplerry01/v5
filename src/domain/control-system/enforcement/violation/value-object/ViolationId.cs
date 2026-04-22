namespace Whycespace.Domain.ControlSystem.Enforcement.Violation;

public readonly record struct ViolationId
{
    public Guid Value { get; }

    public ViolationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ViolationId cannot be empty.", nameof(value));
        Value = value;
    }

    public static ViolationId From(Guid value) => new(value);
}
