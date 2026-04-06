using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record RecoveryFailedEvent(Guid PlanId, string Reason) : DomainEvent;
