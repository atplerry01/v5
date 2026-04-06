using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record RecoveryStartedEvent(Guid PlanId) : DomainEvent;
