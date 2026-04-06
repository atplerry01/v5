using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record ContinuityPlanActivatedEvent(Guid PlanId) : DomainEvent;
