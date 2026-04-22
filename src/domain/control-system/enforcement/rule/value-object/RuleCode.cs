namespace Whycespace.Domain.ControlSystem.Enforcement.Rule;

public readonly record struct RuleCode
{
    public string Value { get; }

    public RuleCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RuleCode cannot be empty.", nameof(value));
        Value = value;
    }

    public static RuleCode From(string value) => new(value);
}