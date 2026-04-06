using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvClosedEvent(Guid SpvId, Guid AuditRecordId) : DomainEvent;
