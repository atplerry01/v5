namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record WalletBoundToIdentityEvent(Guid WalletId, Guid IdentityId) : DomainEvent;
