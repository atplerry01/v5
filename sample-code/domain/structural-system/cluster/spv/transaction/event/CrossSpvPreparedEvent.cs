using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record CrossSpvPreparedEvent(Guid TransactionId, Guid PrepareCorrelationId) : DomainEvent;
