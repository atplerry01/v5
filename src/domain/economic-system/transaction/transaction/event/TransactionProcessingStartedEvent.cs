using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionProcessingStartedEvent(
    TransactionId TransactionId,
    Timestamp ProcessingStartedAt) : DomainEvent;
