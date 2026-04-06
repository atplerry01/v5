using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class PolicyApproval
{
    public Guid ApprovalId { get; }
    public Guid ProposalId { get; }
    public Guid ApproverId { get; }
    public ApproverRole Role { get; }
    public string Signature { get; }
    public DateTimeOffset ApprovedAt { get; }

    private PolicyApproval(Guid approvalId, Guid proposalId, Guid approverId,
        ApproverRole role, string signature, DateTimeOffset approvedAt)
    {
        ApprovalId = approvalId;
        ProposalId = proposalId;
        ApproverId = approverId;
        Role = role;
        Signature = signature;
        ApprovedAt = approvedAt;
    }

    public static PolicyApproval Create(
        Guid proposalId, Guid approverId, ApproverRole role, string signature, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(signature);

        return new PolicyApproval(
            DeterministicIdHelper.FromSeed($"PolicyApproval:{proposalId}:{approverId}:{role.Value}"), proposalId, approverId, role, signature, timestamp);
    }
}
