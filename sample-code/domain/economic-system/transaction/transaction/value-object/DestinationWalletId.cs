namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record DestinationWalletId
{
    public Guid Value { get; }

    public DestinationWalletId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DestinationWalletId cannot be empty.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value.ToString();
}
