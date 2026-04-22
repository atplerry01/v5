using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;

public sealed class PolicyAuditAggregate : AggregateRoot
{
    public PolicyAuditId Id { get; private set; }
    public string PolicyId { get; private set; } = string.Empty;
    public string ActorId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public PolicyAuditCategory Category { get; private set; }
    public string DecisionHash { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public bool IsReviewed { get; private set; }
    public string? ReviewerId { get; private set; }

    private PolicyAuditAggregate() { }

    public static PolicyAuditAggregate Record(
        PolicyAuditId id,
        string policyId,
        string actorId,
        string action,
        PolicyAuditCategory category,
        string decisionHash,
        string correlationId,
        DateTimeOffset occurredAt)
    {
        Guard.Against(string.IsNullOrEmpty(policyId), PolicyAuditErrors.PolicyIdMustNotBeEmpty().Message);
        Guard.Against(string.IsNullOrEmpty(actorId), PolicyAuditErrors.ActorIdMustNotBeEmpty().Message);

        var aggregate = new PolicyAuditAggregate();
        aggregate.RaiseDomainEvent(new PolicyAuditEntryRecordedEvent(
            id, policyId, actorId, action, category, decisionHash, correlationId, occurredAt));
        return aggregate;
    }

    public void Review(string reviewerId, string reason)
    {
        Guard.Against(IsReviewed, PolicyAuditErrors.AuditEntryAlreadyReviewed().Message);
        Guard.Against(string.IsNullOrEmpty(reason), PolicyAuditErrors.ReviewReasonMustNotBeEmpty().Message);

        RaiseDomainEvent(new PolicyAuditEntryReviewedEvent(Id, reviewerId, reason));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PolicyAuditEntryRecordedEvent e:
                Id = e.Id;
                PolicyId = e.PolicyId;
                ActorId = e.ActorId;
                Action = e.Action;
                Category = e.Category;
                DecisionHash = e.DecisionHash;
                CorrelationId = e.CorrelationId;
                OccurredAt = e.OccurredAt;
                break;
            case PolicyAuditEntryReviewedEvent e:
                IsReviewed = true;
                ReviewerId = e.ReviewerId;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "PolicyAudit entry must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(PolicyId), "PolicyAudit entry must have a non-empty PolicyId.");
        Guard.Against(string.IsNullOrEmpty(ActorId), "PolicyAudit entry must have a non-empty ActorId.");
    }
}
