namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record WalletCreatedEvent(Guid WalletId, Guid IdentityId, string CurrencyCode) : DomainEvent;
