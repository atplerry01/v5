namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionRejectedEvent(Guid TransactionId, string Reason) : DomainEvent;
