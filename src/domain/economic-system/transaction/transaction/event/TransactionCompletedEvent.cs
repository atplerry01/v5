using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionCompletedEvent(
    TransactionId TransactionId,
    Guid JournalId,
    Timestamp CompletedAt) : DomainEvent;
