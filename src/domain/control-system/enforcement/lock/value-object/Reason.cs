namespace Whycespace.Domain.ControlSystem.Enforcement.Lock;

public readonly record struct Reason
{
    public string Value { get; }

    public Reason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Reason cannot be empty.", nameof(value));
        Value = value;
    }

    public static Reason From(string value) => new(value);
}
