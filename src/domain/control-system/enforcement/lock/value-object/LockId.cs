namespace Whycespace.Domain.ControlSystem.Enforcement.Lock;

public readonly record struct LockId
{
    public Guid Value { get; }

    public LockId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LockId cannot be empty.", nameof(value));
        Value = value;
    }

    public static LockId From(Guid value) => new(value);
}
