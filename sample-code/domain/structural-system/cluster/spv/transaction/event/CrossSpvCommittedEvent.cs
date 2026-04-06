using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record CrossSpvCommittedEvent(Guid TransactionId, Guid CommitCorrelationId) : DomainEvent;
