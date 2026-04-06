namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed record TransactionCount
{
    public int Value { get; }

    public TransactionCount(int value)
    {
        if (value < 0)
            throw new ArgumentException("Transaction count cannot be negative.", nameof(value));
        Value = value;
    }

    public static TransactionCount Zero => new(0);
    public TransactionCount Add(TransactionCount other) => new(Value + other.Value);
}
