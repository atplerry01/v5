namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionApprovedEvent(
    Guid TransactionId,
    bool RequiresLedgerEntry = true,
    Guid LedgerCorrelationId = default) : DomainEvent;
