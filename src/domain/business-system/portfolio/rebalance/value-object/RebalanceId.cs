namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public readonly record struct RebalanceId
{
    public Guid Value { get; }

    public RebalanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RebalanceId value must not be empty.", nameof(value));

        Value = value;
    }
}
