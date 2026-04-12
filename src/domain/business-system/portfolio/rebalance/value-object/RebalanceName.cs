namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public readonly record struct RebalanceName
{
    public string Value { get; }

    public RebalanceName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RebalanceName must not be empty.", nameof(value));

        Value = value;
    }
}
