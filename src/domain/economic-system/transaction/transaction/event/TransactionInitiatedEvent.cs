using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionInitiatedEvent(
    TransactionId TransactionId,
    string Kind,
    IReadOnlyList<TransactionReference> References,
    Timestamp InitiatedAt) : DomainEvent;
