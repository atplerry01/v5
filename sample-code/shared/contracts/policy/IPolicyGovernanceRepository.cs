namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyGovernanceRepository
{
    Task CreateProposalAsync(PolicyProposalRecord record, CancellationToken cancellationToken = default);
    Task<PolicyProposalRecord?> GetProposalAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task UpdateProposalStatusAsync(Guid proposalId, string status, CancellationToken cancellationToken = default);
    Task SaveApprovalAsync(PolicyApprovalRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyApprovalRecord>> GetApprovalsAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task CreateActivationRecordAsync(PolicyActivationRecord record, CancellationToken cancellationToken = default);
    Task<PolicyActivationRecord?> GetActivationByVersionAsync(Guid policyVersionId, CancellationToken cancellationToken = default);
    Task UpdateActivationChainTxAsync(Guid activationId, string chainTxId, CancellationToken cancellationToken = default);
}

public sealed record PolicyProposalRecord(
    Guid Id,
    Guid PolicyId,
    Guid ProposedBy,
    string DslContent,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record PolicyApprovalRecord(
    Guid Id,
    Guid ProposalId,
    Guid ApproverId,
    string Decision,
    DateTimeOffset CreatedAt);

public sealed record PolicyActivationRecord(
    Guid Id,
    Guid PolicyVersionId,
    Guid ActivatedBy,
    string ActivationHash,
    DateTimeOffset CreatedAt,
    string? ChainTxId = null);
