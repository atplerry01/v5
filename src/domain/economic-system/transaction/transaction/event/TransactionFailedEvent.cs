using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionFailedEvent(
    TransactionId TransactionId,
    string Reason,
    Timestamp FailedAt) : DomainEvent;
