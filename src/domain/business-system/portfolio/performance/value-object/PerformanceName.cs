namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public readonly record struct PerformanceName
{
    public string Value { get; }

    public PerformanceName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PerformanceName must not be empty.", nameof(value));

        Value = value;
    }
}
