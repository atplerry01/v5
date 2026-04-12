using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionInitiatedEvent(
    TransactionId TransactionId,
    Guid InstructionId,
    Timestamp InitiatedAt) : DomainEvent;
