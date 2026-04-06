using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record CrossSpvCreatedEvent(Guid TransactionId, Guid RootSpvId) : DomainEvent;
