using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.DecisionSystem.Governance.Dispute;

public sealed class DisputeCaseAggregate : AggregateRoot
{
    public DisputeId DisputeId { get; private set; }
    public Guid RelatedDecisionId { get; private set; }
    public IdentityId InitiatorIdentityId { get; private set; } = default!;
    public IdentityId? ReviewerIdentityId { get; private set; }
    public DisputeStatus Status { get; private set; }
    public DisputeOutcome Outcome { get; private set; }

    private DisputeCaseAggregate() { }

    public static DisputeCaseAggregate RaiseDispute(Guid relatedDecisionId, IdentityId initiatorIdentityId)
    {
        var dispute = new DisputeCaseAggregate
        {
            DisputeId = DisputeId.FromSeed($"DisputeCase:{relatedDecisionId}:{initiatorIdentityId.Value}"),
            RelatedDecisionId = relatedDecisionId,
            InitiatorIdentityId = initiatorIdentityId,
            Status = DisputeStatus.Open,
            Outcome = DisputeOutcome.None
        };

        dispute.Id = dispute.DisputeId;

        dispute.RaiseDomainEvent(new DisputeRaisedEvent(
            dispute.DisputeId,
            relatedDecisionId,
            initiatorIdentityId.Value));

        return dispute;
    }

    public void AssignReviewer(IdentityId reviewerIdentityId)
    {
        if (Status != DisputeStatus.Open)
            throw new InvalidOperationException("Only open disputes can be assigned a reviewer.");

        if (reviewerIdentityId == InitiatorIdentityId)
            throw new InvalidOperationException("The initiator cannot review their own dispute.");

        ReviewerIdentityId = reviewerIdentityId;
        Status = DisputeStatus.UnderReview;
    }

    public void ResolveDispute(DisputeOutcome outcome, DisputeStatus resolution)
    {
        if (Status != DisputeStatus.UnderReview)
            throw new InvalidOperationException("Only disputes under review can be resolved.");

        if (resolution is not (DisputeStatus.Resolved or DisputeStatus.Rejected))
            throw new ArgumentException("Resolution must be Resolved or Rejected.", nameof(resolution));

        if (!outcome.HasVerdict)
            throw new ArgumentException("A verdict is required to resolve a dispute.", nameof(outcome));

        Outcome = outcome;
        Status = resolution;

        RaiseDomainEvent(new DisputeResolvedEvent(
            DisputeId,
            resolution,
            outcome.Verdict));
    }
}
