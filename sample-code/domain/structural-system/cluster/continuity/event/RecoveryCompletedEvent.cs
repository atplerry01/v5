using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record RecoveryCompletedEvent(Guid PlanId) : DomainEvent;
