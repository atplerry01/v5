namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record SourceWalletId
{
    public Guid Value { get; }

    public SourceWalletId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SourceWalletId cannot be empty.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value.ToString();
}
