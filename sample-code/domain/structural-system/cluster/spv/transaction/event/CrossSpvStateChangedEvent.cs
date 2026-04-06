using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record CrossSpvStateChangedEvent(
    Guid TransactionId,
    string NewState) : DomainEvent;
