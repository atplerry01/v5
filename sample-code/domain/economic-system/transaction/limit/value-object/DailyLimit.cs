namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record DailyLimit
{
    public Amount Value { get; }

    public DailyLimit(Amount value)
    {
        if (!value.IsPositive)
            throw new ArgumentException("DailyLimit must be positive.", nameof(value));

        Value = value;
    }

    public bool IsExceededBy(Amount dailyTotal) => dailyTotal > Value;

    public override string ToString() => Value.ToString();
}
