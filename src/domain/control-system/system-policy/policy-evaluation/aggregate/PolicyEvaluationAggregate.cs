using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;

public sealed class PolicyEvaluationAggregate : AggregateRoot
{
    public PolicyEvaluationId Id { get; private set; }
    public string PolicyId { get; private set; } = string.Empty;
    public string ActorId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public EvaluationOutcome? Outcome { get; private set; }
    public string? DecisionHash { get; private set; }

    private PolicyEvaluationAggregate() { }

    public static PolicyEvaluationAggregate Record(
        PolicyEvaluationId id,
        string policyId,
        string actorId,
        string action,
        string correlationId)
    {
        Guard.Against(string.IsNullOrEmpty(policyId), PolicyEvaluationErrors.PolicyIdMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(actorId), PolicyEvaluationErrors.ActorIdMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(action), PolicyEvaluationErrors.ActionMustNotBeEmpty().Message);

        var aggregate = new PolicyEvaluationAggregate();
        aggregate.RaiseDomainEvent(new PolicyEvaluationRecordedEvent(id, policyId, actorId, action, correlationId));
        return aggregate;
    }

    public void IssueVerdict(EvaluationOutcome outcome, string decisionHash)
    {
        Guard.Against(Outcome.HasValue, PolicyEvaluationErrors.VerdictAlreadyIssued().Message);
        Guard.Against(string.IsNullOrEmpty(decisionHash), PolicyEvaluationErrors.DecisionHashMustNotBeEmpty().Message);

        RaiseDomainEvent(new PolicyEvaluationVerdictIssuedEvent(Id, outcome, decisionHash));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PolicyEvaluationRecordedEvent e:
                Id = e.Id;
                PolicyId = e.PolicyId;
                ActorId = e.ActorId;
                Action = e.Action;
                CorrelationId = e.CorrelationId;
                break;
            case PolicyEvaluationVerdictIssuedEvent e:
                Outcome = e.Outcome;
                DecisionHash = e.DecisionHash;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "PolicyEvaluation must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(PolicyId), "PolicyEvaluation must have a non-empty PolicyId.");
        Guard.Against(string.IsNullOrEmpty(ActorId), "PolicyEvaluation must have a non-empty ActorId.");
    }
}
