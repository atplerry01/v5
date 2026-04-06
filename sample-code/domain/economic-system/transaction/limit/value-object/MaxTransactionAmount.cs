namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record MaxTransactionAmount
{
    public Amount Value { get; }

    public MaxTransactionAmount(Amount value)
    {
        if (!value.IsPositive)
            throw new ArgumentException("MaxTransactionAmount must be positive.", nameof(value));

        Value = value;
    }

    public bool IsExceededBy(Amount transactionAmount) => transactionAmount > Value;

    public override string ToString() => Value.ToString();
}
