namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public readonly record struct TransactionId
{
    public Guid Value { get; }

    public TransactionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransactionId cannot be empty.", nameof(value));
        Value = value;
    }

    public static TransactionId From(Guid value) => new(value);
}
