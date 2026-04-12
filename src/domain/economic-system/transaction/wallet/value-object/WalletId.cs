namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public readonly record struct WalletId
{
    public Guid Value { get; }

    public WalletId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WalletId cannot be empty.", nameof(value));
        Value = value;
    }

    public static WalletId From(Guid value) => new(value);
}
