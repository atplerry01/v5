namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record WalletFrozenEvent(Guid WalletId, string Reason) : DomainEvent;
