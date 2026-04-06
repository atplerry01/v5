namespace Whycespace.Domain.OperationalSystem.Sandbox.Todo;

public sealed record TodoPriority
{
    public int Value { get; }

    public TodoPriority(int value)
    {
        Guard.AgainstInvalid(value, v => v >= 0 && v <= 5, "Priority must be between 0 and 5", nameof(value));
        Value = value;
    }

    public bool IsHighPriority => Value >= 4;

    public static TodoPriority Low => new(1);
    public static TodoPriority Normal => new(2);
    public static TodoPriority High => new(4);
    public static TodoPriority Critical => new(5);
}
