using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed class ContinuityPlanAggregate : AggregateRoot
{
    public Guid ClusterId { get; private set; }
    public PlanType PlanType { get; private set; } = null!;
    public ContinuityStatus Status { get; private set; } = ContinuityStatus.Draft;
    public Guid? ActiveFailoverTargetId { get; private set; }
    public string? FailureReason { get; private set; }

    private readonly List<FailoverTarget> _failoverTargets = [];
    public IReadOnlyCollection<FailoverTarget> FailoverTargets => _failoverTargets.AsReadOnly();

    private readonly List<RecoveryStep> _recoverySteps = [];
    public IReadOnlyCollection<RecoveryStep> RecoverySteps => _recoverySteps.AsReadOnly();

    public ContinuityPlanAggregate() { }

    public static ContinuityPlanAggregate Create(
        Guid id,
        Guid clusterId,
        PlanType planType)
    {
        if (id == Guid.Empty)
            throw new ContinuityException("CONTINUITY_ID_REQUIRED", "Continuity plan id required.");

        if (clusterId == Guid.Empty)
            throw new ContinuityException("CONTINUITY_CLUSTER_REQUIRED", "Cluster id required.");

        if (planType is null || string.IsNullOrWhiteSpace(planType.Code))
            throw new ContinuityException("CONTINUITY_PLAN_TYPE_REQUIRED", "Plan type required.");

        var aggregate = new ContinuityPlanAggregate
        {
            Id = id,
            ClusterId = clusterId,
            PlanType = planType,
            Status = ContinuityStatus.Draft
        };

        aggregate.RaiseDomainEvent(new ContinuityPlanCreatedEvent(id, clusterId, planType.Code));

        return aggregate;
    }

    public void AddFailoverTarget(FailoverTarget target)
    {
        EnsureInvariant(
            Status == ContinuityStatus.Draft,
            "PLAN_MUST_BE_DRAFT",
            "Failover targets can only be added while the plan is in draft.");

        EnsureInvariant(
            target is not null,
            "TARGET_REQUIRED",
            "Failover target is required.");

        _failoverTargets.Add(target!);
    }

    public void AddRecoveryStep(RecoveryStep step)
    {
        EnsureInvariant(
            Status == ContinuityStatus.Draft,
            "PLAN_MUST_BE_DRAFT",
            "Recovery steps can only be added while the plan is in draft.");

        EnsureInvariant(
            step is not null,
            "STEP_REQUIRED",
            "Recovery step is required.");

        _recoverySteps.Add(step!);
    }

    public void Activate()
    {
        EnsureInvariant(
            _failoverTargets.Count > 0,
            "MIN_FAILOVER_TARGETS",
            "Plan must have at least one failover target.");

        EnsureInvariant(
            _recoverySteps.Count > 0,
            "MIN_RECOVERY_STEPS",
            "Plan must have at least one recovery step.");

        EnsureValidTransition(Status, ContinuityStatus.Active,
            (from, to) => from.CanTransitionTo(to));

        Status = ContinuityStatus.Active;
        RaiseDomainEvent(new ContinuityPlanActivatedEvent(Id));
    }

    public void TriggerFailover(Guid targetId, string reason)
    {
        EnsureInvariant(
            Status == ContinuityStatus.Active,
            "PLAN_MUST_BE_ACTIVE",
            "Only active plans can trigger failover.");

        EnsureInvariant(
            _failoverTargets.Any(t => t.TargetId == targetId),
            "TARGET_MUST_EXIST",
            "Failover target must belong to this plan.");

        EnsureValidTransition(Status, ContinuityStatus.Triggered,
            (from, to) => from.CanTransitionTo(to));

        Status = ContinuityStatus.Triggered;
        ActiveFailoverTargetId = targetId;
        RaiseDomainEvent(new FailoverTriggeredEvent(Id, targetId, reason));
    }

    public void StartRecovery()
    {
        EnsureValidTransition(Status, ContinuityStatus.Recovering,
            (from, to) => from.CanTransitionTo(to));

        Status = ContinuityStatus.Recovering;
        RaiseDomainEvent(new RecoveryStartedEvent(Id));
    }

    public void CompleteRecovery()
    {
        EnsureValidTransition(Status, ContinuityStatus.Completed,
            (from, to) => from.CanTransitionTo(to));

        Status = ContinuityStatus.Completed;
        RaiseDomainEvent(new RecoveryCompletedEvent(Id));
    }

    public void FailRecovery(string reason)
    {
        EnsureValidTransition(Status, ContinuityStatus.Failed,
            (from, to) => from.CanTransitionTo(to));

        Status = ContinuityStatus.Failed;
        FailureReason = reason;
        RaiseDomainEvent(new RecoveryFailedEvent(Id, reason));
    }
}
