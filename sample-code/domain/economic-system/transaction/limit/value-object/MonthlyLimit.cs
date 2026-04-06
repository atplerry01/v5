namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record MonthlyLimit
{
    public Amount Value { get; }

    public MonthlyLimit(Amount value)
    {
        if (!value.IsPositive)
            throw new ArgumentException("MonthlyLimit must be positive.", nameof(value));

        Value = value;
    }

    public bool IsExceededBy(Amount monthlyTotal) => monthlyTotal > Value;

    public override string ToString() => Value.ToString();
}
