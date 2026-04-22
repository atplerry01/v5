namespace Whycespace.Domain.ControlSystem.Enforcement.Rule;

public readonly record struct RuleCategory
{
    public string Value { get; }

    public RuleCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RuleCategory cannot be empty.", nameof(value));
        Value = value;
    }

    public static RuleCategory From(string value) => new(value);
}