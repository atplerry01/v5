namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionCompletedEvent(Guid TransactionId) : DomainEvent;
