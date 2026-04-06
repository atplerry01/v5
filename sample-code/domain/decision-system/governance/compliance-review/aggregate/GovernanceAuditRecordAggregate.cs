using Whycespace.Domain.SharedKernel;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.DecisionSystem.Governance.ComplianceReview;

/// <summary>
/// Immutable audit trail for governance decisions.
/// Links every decision back to its originating proposal and ballot,
/// ensuring full traceability of who participated and what was decided.
/// </summary>
public sealed class GovernanceAuditRecordAggregate : AggregateRoot
{
    public AuditId AuditId { get; }
    public Guid ProposalId { get; }
    public Guid BallotId { get; }
    public DecisionType DecisionType { get; }
    public DecisionOutcome DecisionOutcome { get; }
    public IReadOnlyList<Guid> Participants { get; }
    public DateTimeOffset Timestamp { get; }

    private GovernanceAuditRecordAggregate(
        AuditId auditId,
        Guid proposalId,
        Guid ballotId,
        DecisionType decisionType,
        DecisionOutcome decisionOutcome,
        IReadOnlyList<Guid> participants,
        DateTimeOffset timestamp)
    {
        AuditId = auditId;
        Id = auditId;
        ProposalId = proposalId;
        BallotId = ballotId;
        DecisionType = decisionType;
        DecisionOutcome = decisionOutcome;
        Participants = participants;
        Timestamp = timestamp;
    }

    public static GovernanceAuditRecordAggregate RecordDecision(
        Guid proposalId,
        Guid ballotId,
        DecisionType decisionType,
        DecisionOutcome decisionOutcome,
        IReadOnlyList<Guid> participants,
        DateTimeOffset timestamp)
    {
        var auditId = AuditId.FromSeed($"GovernanceAuditRecord:{proposalId}:{ballotId}:{decisionType.Value}");

        var record = new GovernanceAuditRecordAggregate(
            auditId,
            proposalId,
            ballotId,
            decisionType,
            decisionOutcome,
            participants,
            timestamp);

        record.RaiseDomainEvent(new GovernanceDecisionRecordedEvent(
            auditId,
            proposalId,
            ballotId,
            decisionType.Value,
            decisionOutcome.Value,
            participants,
            timestamp));

        return record;
    }
}
