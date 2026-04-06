namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionInitiatedEvent(
    Guid TransactionId,
    Guid SourceWalletId,
    Guid DestinationWalletId,
    decimal Amount,
    string CurrencyCode) : DomainEvent;
