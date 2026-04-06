namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed class PlanReadySpecification
{
    public bool IsSatisfiedBy(ContinuityPlanAggregate plan)
        => plan.FailoverTargets.Count > 0 && plan.RecoverySteps.Count > 0;
}
