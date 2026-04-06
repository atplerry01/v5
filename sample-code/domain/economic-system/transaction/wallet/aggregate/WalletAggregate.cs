namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public class WalletAggregate : AggregateRoot
{
    private IdentityId _ownerId = null!;
    private WalletStatus _status;
    private Balance _balance = null!;

    public void Create(Guid id, IdentityId ownerId, Currency currency)
    {
        EnsureInvariant(id != Guid.Empty, "WalletId", "WalletId cannot be empty.");
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(currency);

        Id = id;
        _ownerId = ownerId;
        _status = WalletStatus.Active;
        _balance = Balance.Zero(currency);

        RaiseDomainEvent(new WalletCreatedEvent(id, ownerId.Value, currency.Code));
        RaiseDomainEvent(new WalletBoundToIdentityEvent(id, ownerId.Value));
    }

    public void Freeze(string reason)
    {
        EnsureInvariant(_status == WalletStatus.Active, "WalletActive", "Only active wallets can be frozen.");

        _status = WalletStatus.Frozen;
        RaiseDomainEvent(new WalletFrozenEvent(Id, reason));
    }

    public void Unfreeze()
    {
        EnsureInvariant(_status == WalletStatus.Frozen, "WalletFrozen", "Only frozen wallets can be unfrozen.");

        _status = WalletStatus.Active;
        RaiseDomainEvent(new WalletUnfrozenEvent(Id));
    }
}
