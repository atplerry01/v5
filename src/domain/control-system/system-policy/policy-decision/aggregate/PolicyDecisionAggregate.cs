using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDecision;

public sealed class PolicyDecisionAggregate : AggregateRoot
{
    public PolicyDecisionId Id { get; private set; }
    public string PolicyDefinitionId { get; private set; } = string.Empty;
    public string SubjectId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public PolicyDecisionOutcome Outcome { get; private set; }
    public DateTimeOffset DecidedAt { get; private set; }

    private PolicyDecisionAggregate() { }

    public static PolicyDecisionAggregate Record(
        PolicyDecisionId id,
        string policyDefinitionId,
        string subjectId,
        string action,
        string resource,
        PolicyDecisionOutcome outcome,
        DateTimeOffset decidedAt)
    {
        Guard.Against(string.IsNullOrEmpty(policyDefinitionId), "PolicyDecision requires a policyDefinitionId.");
        Guard.Against(string.IsNullOrEmpty(subjectId), "PolicyDecision requires a subjectId.");
        Guard.Against(string.IsNullOrEmpty(action), "PolicyDecision requires an action.");
        Guard.Against(string.IsNullOrEmpty(resource), "PolicyDecision requires a resource.");

        var aggregate = new PolicyDecisionAggregate();
        aggregate.RaiseDomainEvent(new PolicyDecisionRecordedEvent(
            id, policyDefinitionId, subjectId, action, resource, outcome, decidedAt));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        if (domainEvent is PolicyDecisionRecordedEvent e)
        {
            Id = e.Id;
            PolicyDefinitionId = e.PolicyDefinitionId;
            SubjectId = e.SubjectId;
            Action = e.Action;
            Resource = e.Resource;
            Outcome = e.Outcome;
            DecidedAt = e.DecidedAt;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "PolicyDecision must have a non-empty Id.");
    }
}
