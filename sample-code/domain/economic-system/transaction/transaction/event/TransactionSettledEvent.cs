namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionSettledEvent(Guid TransactionId) : DomainEvent;
