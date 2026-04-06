namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public sealed record Reason
{
    public string Value { get; }

    public Reason(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public override string ToString() => Value;
}
