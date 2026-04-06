using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record FailoverTriggeredEvent(Guid PlanId, Guid TargetId, string Reason) : DomainEvent;
