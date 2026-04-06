namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed class ContinuityException : DomainException
{
    public ContinuityException(string code, string message) : base(code, message) { }

    public static ContinuityException IncompletePlan() =>
        new("CONTINUITY_INCOMPLETE_PLAN", "Plan must have at least one failover target and one recovery step before activation.");

    public static ContinuityException PlanNotActive() =>
        new("CONTINUITY_PLAN_NOT_ACTIVE", "Plan must be in active state to trigger failover.");

    public static ContinuityException RecoveryAlreadyInProgress() =>
        new("CONTINUITY_RECOVERY_IN_PROGRESS", "Recovery is already in progress for this plan.");
}
