using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record ContinuityPlanCreatedEvent(Guid PlanId, Guid ClusterId, string PlanType) : DomainEvent;
