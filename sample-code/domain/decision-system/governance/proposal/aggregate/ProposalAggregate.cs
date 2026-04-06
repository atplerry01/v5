using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.DecisionSystem.Governance.Proposal;

public class ProposalAggregate : AggregateRoot
{
    public IdentityId ProposerIdentityId { get; private set; } = default!;
    public ProposalStatus Status { get; private set; } = ProposalStatus.Pending;

    protected ProposalAggregate() { }

    public static ProposalAggregate Create(Guid proposalId, IdentityId proposerIdentityId)
    {
        Guard.AgainstDefault(proposalId);
        Guard.AgainstNull(proposerIdentityId);

        var proposal = new ProposalAggregate();
        proposal.Apply(new ProposalCreatedEvent(proposalId, proposerIdentityId.Value));
        return proposal;
    }

    public void Approve()
    {
        EnsureInvariant(
            Status == ProposalStatus.Pending,
            "INVALID_STATE_TRANSITION",
            $"Cannot approve proposal in '{Status}' status.");

        Apply(new ProposalApprovedEvent(Id));
    }

    public void Reject()
    {
        EnsureInvariant(
            Status == ProposalStatus.Pending,
            "INVALID_STATE_TRANSITION",
            $"Cannot reject proposal in '{Status}' status.");

        Apply(new ProposalRejectedEvent(Id));
    }

    private void Apply(ProposalCreatedEvent e)
    {
        Id = e.ProposalId;
        ProposerIdentityId = new IdentityId(e.ProposerIdentityId);
        Status = ProposalStatus.Pending;
        RaiseDomainEvent(e);
    }

    private void Apply(ProposalApprovedEvent e)
    {
        Status = ProposalStatus.Approved;
        RaiseDomainEvent(e);
    }

    private void Apply(ProposalRejectedEvent e)
    {
        Status = ProposalStatus.Rejected;
        RaiseDomainEvent(e);
    }
}
