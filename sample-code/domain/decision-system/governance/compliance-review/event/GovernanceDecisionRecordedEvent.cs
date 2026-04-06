using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.ComplianceReview;

public sealed record GovernanceDecisionRecordedEvent(
    Guid AuditRecordId,
    Guid ProposalId,
    Guid BallotId,
    string DecisionType,
    string DecisionOutcome,
    IReadOnlyList<Guid> ParticipantIds,
    DateTimeOffset DecisionTimestamp
) : DomainEvent;
