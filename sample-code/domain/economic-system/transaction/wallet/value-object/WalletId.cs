using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record WalletId
{
    public Guid Value { get; }

    public WalletId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WalletId cannot be empty.", nameof(value));

        Value = value;
    }

    public static WalletId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public override string ToString() => Value.ToString();
}
