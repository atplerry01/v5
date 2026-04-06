namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed class WalletOwnership : Entity
{
    public Guid WalletId { get; private set; }
    public IdentityId OwnerId { get; private set; } = null!;
    public DateTimeOffset BoundAt { get; private set; }

    public static WalletOwnership Create(Guid id, Guid walletId, IdentityId ownerId, DateTimeOffset timestamp)
    {
        return new WalletOwnership
        {
            Id = id,
            WalletId = walletId,
            OwnerId = ownerId,
            BoundAt = timestamp
        };
    }
}
