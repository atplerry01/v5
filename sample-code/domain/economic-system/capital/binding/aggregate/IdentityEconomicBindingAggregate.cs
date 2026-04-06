namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public class IdentityEconomicBindingAggregate : AggregateRoot
{
    private IdentityId _identityId = null!;
    private Guid _walletId;
    private BindingStatus _status;

    public void Create(Guid id, IdentityId identityId, Guid walletId)
    {
        EnsureInvariant(id != Guid.Empty, "BindingId", "BindingId cannot be empty.");
        ArgumentNullException.ThrowIfNull(identityId);
        EnsureInvariant(walletId != Guid.Empty, "WalletId", "WalletId cannot be empty.");

        Id = id;
        _identityId = identityId;
        _walletId = walletId;
        _status = BindingStatus.Active;

        RaiseDomainEvent(new IdentityWalletBoundEvent(id, identityId.Value, walletId));
    }

    public void Revoke()
    {
        EnsureInvariant(_status == BindingStatus.Active, "BindingActive", "Only active bindings can be revoked.");

        _status = BindingStatus.Revoked;
        RaiseDomainEvent(new IdentityWalletUnboundEvent(Id, _identityId.Value, _walletId));
    }
}
