using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class PolicyProposalAggregate : AggregateRoot
{
    public Guid PolicyId { get; private set; }
    public int ProposedVersion { get; private set; }
    public Guid ProposedBy { get; private set; }
    public PolicyChangeType ChangeType { get; private set; } = default!;
    public ProposalStatus Status { get; private set; } = default!;
    public string Metadata { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<PolicyApproval> _approvals = [];
    public IReadOnlyList<PolicyApproval> Approvals => _approvals.AsReadOnly();

    private PolicyProposalAggregate() { }

    public static PolicyProposalAggregate Create(
        Guid proposalId, Guid policyId, int proposedVersion,
        Guid proposedBy, PolicyChangeType changeType, string metadata,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(changeType);

        var proposal = new PolicyProposalAggregate
        {
            Id = proposalId,
            PolicyId = policyId,
            ProposedVersion = proposedVersion,
            ProposedBy = proposedBy,
            ChangeType = changeType,
            Status = ProposalStatus.Draft,
            Metadata = metadata,
            CreatedAt = timestamp
        };

        proposal.RaiseDomainEvent(new PolicyProposalCreatedEvent(proposalId, policyId, proposedVersion));
        return proposal;
    }

    public void Submit()
    {
        EnsureInvariant(Status == ProposalStatus.Draft, "DraftOnly", "Only draft proposals can be submitted.");
        Status = ProposalStatus.Active;
    }

    public void AddApproval(PolicyApproval approval)
    {
        ArgumentNullException.ThrowIfNull(approval);
        EnsureInvariant(Status == ProposalStatus.Active, "ActiveOnly", "Only active proposals accept approvals.");
        EnsureInvariant(!_approvals.Any(a => a.ApproverId == approval.ApproverId),
            "UniqueApprover", "This approver has already voted.");

        _approvals.Add(approval);
        RaiseDomainEvent(new PolicyProposalApprovedEvent(Id, approval.ApproverId));
    }

    public void Approve()
    {
        EnsureInvariant(Status == ProposalStatus.Active, "ActiveOnly", "Only active proposals can be approved.");
        Status = ProposalStatus.Approved;
    }

    public void Reject(string reason)
    {
        EnsureInvariant(Status == ProposalStatus.Active, "ActiveOnly", "Only active proposals can be rejected.");
        Status = ProposalStatus.Rejected;
    }

    public void MarkExecuted()
    {
        EnsureInvariant(Status == ProposalStatus.Approved, "ApprovedOnly", "Only approved proposals can be executed.");
        Status = ProposalStatus.Executed;
    }
}
