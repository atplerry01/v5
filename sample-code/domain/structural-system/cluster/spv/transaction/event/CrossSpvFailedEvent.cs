using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record CrossSpvFailedEvent(
    Guid TransactionId,
    Guid FailCorrelationId,
    string Reason) : DomainEvent;
