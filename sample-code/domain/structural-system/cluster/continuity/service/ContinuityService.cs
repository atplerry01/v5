namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

/// <summary>
/// Continuity domain service — validates continuity plan lifecycle transitions
/// and enforces business rules for failover and recovery operations.
/// </summary>
public sealed class ContinuityService
{
    private readonly PlanReadySpecification _readySpec = new();

    /// <summary>
    /// Validates that a continuity plan can be activated — must satisfy readiness specification.
    /// </summary>
    public ContinuityResult ValidateActivation(ContinuityPlanAggregate plan)
    {
        if (plan.Status != ContinuityStatus.Draft)
            return ContinuityResult.Fail($"Cannot activate plan in '{plan.Status.Value}' state. Must be in draft.");

        if (!_readySpec.IsSatisfiedBy(plan))
            return ContinuityResult.Fail("Plan must have at least one failover target and one recovery step.");

        return ContinuityResult.Success();
    }

    /// <summary>
    /// Validates that a failover can be triggered for the plan.
    /// </summary>
    public ContinuityResult ValidateTrigger(ContinuityPlanAggregate plan)
    {
        if (plan.Status != ContinuityStatus.Active)
            return ContinuityResult.Fail("Plan must be active to trigger failover.");

        return ContinuityResult.Success();
    }
}

public sealed record ContinuityResult(bool IsValid, string? Error)
{
    public static ContinuityResult Fail(string error) => new(false, error);
    public static ContinuityResult Success() => new(true, null);
}
