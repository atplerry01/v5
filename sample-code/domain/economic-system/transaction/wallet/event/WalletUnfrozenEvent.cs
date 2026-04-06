namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record WalletUnfrozenEvent(Guid WalletId) : DomainEvent;
