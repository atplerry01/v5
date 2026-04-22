using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEnforcement;

public sealed class PolicyEnforcementAggregate : AggregateRoot
{
    public PolicyEnforcementId Id { get; private set; }
    public string PolicyDecisionId { get; private set; } = string.Empty;
    public string TargetId { get; private set; } = string.Empty;
    public PolicyEnforcementOutcome Outcome { get; private set; }
    public bool IsNoPolicyFlagAnomaly { get; private set; }
    public DateTimeOffset EnforcedAt { get; private set; }

    private PolicyEnforcementAggregate() { }

    public static PolicyEnforcementAggregate Record(
        PolicyEnforcementId id,
        string policyDecisionId,
        string targetId,
        PolicyEnforcementOutcome outcome,
        DateTimeOffset enforcedAt,
        bool isNoPolicyFlagAnomaly = false)
    {
        Guard.Against(string.IsNullOrEmpty(policyDecisionId), "PolicyEnforcement requires a policyDecisionId.");
        Guard.Against(string.IsNullOrEmpty(targetId), "PolicyEnforcement requires a targetId.");
        Guard.Against(
            isNoPolicyFlagAnomaly && outcome != PolicyEnforcementOutcome.Bypassed,
            "NoPolicyFlag anomaly must result in a Bypassed outcome.");

        var aggregate = new PolicyEnforcementAggregate();
        aggregate.RaiseDomainEvent(new PolicyEnforcedEvent(
            id, policyDecisionId, targetId, outcome, enforcedAt, isNoPolicyFlagAnomaly));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        if (domainEvent is PolicyEnforcedEvent e)
        {
            Id = e.Id;
            PolicyDecisionId = e.PolicyDecisionId;
            TargetId = e.TargetId;
            Outcome = e.Outcome;
            EnforcedAt = e.EnforcedAt;
            IsNoPolicyFlagAnomaly = e.IsNoPolicyFlagAnomaly;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "PolicyEnforcement must have a non-empty Id.");
    }
}
