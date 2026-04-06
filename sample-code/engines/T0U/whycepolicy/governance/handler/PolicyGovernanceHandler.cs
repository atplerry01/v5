using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhycePolicy.Governance;

/// <summary>
/// Stateless governance engine. Reads from IPolicyReadModel (projections).
/// Does NOT write — returns validation results with persistence payloads
/// for the runtime/orchestrator to handle.
/// </summary>
public sealed class PolicyGovernanceHandler : IPolicyGovernanceEngine
{
    private readonly IPolicyReadModel _readModel;
    private readonly IClock _clock;
    private readonly GovernanceQuorum _defaultQuorum;
    private readonly IArtifactIntegrityVerifier? _artifactIntegrityVerifier;

    public PolicyGovernanceHandler(
        IPolicyReadModel readModel,
        IClock clock,
        GovernanceQuorum? defaultQuorum = null,
        IArtifactIntegrityVerifier? artifactIntegrityVerifier = null)
    {
        ArgumentNullException.ThrowIfNull(readModel);
        _readModel = readModel;
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _defaultQuorum = defaultQuorum ?? GovernanceQuorum.Create(2, ["Guardian"]);
        _artifactIntegrityVerifier = artifactIntegrityVerifier;
    }

    public Task<GovernanceCheckResult> CheckGovernanceAsync(
        Guid policyId, string action, CancellationToken cancellationToken = default)
    {
        var isTier0 = action.StartsWith("policy.", StringComparison.OrdinalIgnoreCase)
            || action.StartsWith("governance.", StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(new GovernanceCheckResult(
            RequiresApproval: true,
            IsTier0Action: isTier0,
            Reason: isTier0 ? "Tier-0 action requires multi-party approval" : "Policy change requires governance"));
    }

    public Task<GovernanceValidationResult> ValidateProposalAsync(
        PolicyProposalInput proposal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);

        var record = new PolicyProposalRecord(
            proposal.Id,
            proposal.PolicyId,
            proposal.ProposedBy,
            proposal.Metadata,
            "Submitted",
            proposal.CreatedAt);

        return Task.FromResult(GovernanceValidationResult.Valid(record));
    }

    public async Task<GovernanceValidationResult> ValidateApprovalAsync(
        PolicyApprovalInput approval, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approval);

        var proposalRecord = await _readModel.GetProposalAsync(approval.ProposalId, cancellationToken);
        if (proposalRecord is null)
            return GovernanceValidationResult.Invalid($"Proposal {approval.ProposalId} not found.");

        var existingApprovals = await _readModel.GetApprovalsAsync(approval.ProposalId, cancellationToken);
        if (existingApprovals.Any(a => a.ApproverId == approval.ApproverId))
            return GovernanceValidationResult.Invalid(
                $"Approver {approval.ApproverId} has already voted on proposal {approval.ProposalId}.");

        var approvalRecord = new PolicyApprovalRecord(
            DeterministicIdHelper.FromSeed($"PolicyApproval:{approval.ProposalId}:{approval.ApproverId}"),
            approval.ProposalId,
            approval.ApproverId,
            "Approve",
            _clock.UtcNowOffset);

        return GovernanceValidationResult.Valid(approvalRecord);
    }

    public async Task<GovernanceActivationResult> ValidateActivationAsync(
        Guid proposalId, CancellationToken cancellationToken = default)
    {
        var proposalRecord = await _readModel.GetProposalAsync(proposalId, cancellationToken);
        if (proposalRecord is null)
            return GovernanceActivationResult.Failed("Proposal not found");

        var approvals = await _readModel.GetApprovalsAsync(proposalId, cancellationToken);

        var uniqueApprovers = approvals.Select(a => a.ApproverId).Distinct().Count();
        if (uniqueApprovers != approvals.Count)
            return GovernanceActivationResult.Failed("Duplicate approvals detected — data integrity violation");

        if (approvals.Any(a => a.Decision == "Reject"))
            return GovernanceActivationResult.Failed("Proposal has blocking rejection");

        if (!ValidateQuorum(approvals))
            return GovernanceActivationResult.Failed("Quorum not met");

        var versions = await _readModel.GetVersionsByPolicyAsync(proposalRecord.PolicyId, cancellationToken);
        var draftVersion = versions.LastOrDefault(v => v.Status == "Draft");

        if (draftVersion is null)
            return GovernanceActivationResult.Failed("No draft version to activate");

        if (draftVersion.ArtifactHash is not null)
        {
            var artifactValid = await VerifyArtifactIntegrityAsync(draftVersion, cancellationToken);
            if (!artifactValid)
                return GovernanceActivationResult.Failed("Artifact integrity verification failed — hash mismatch");
        }

        var activationHash = ComputeActivationHash(proposalRecord.PolicyId, draftVersion.Version);

        return GovernanceActivationResult.Succeeded(
            proposalRecord.PolicyId, draftVersion.Version, activationHash,
            draftVersion.Id, proposalRecord.ProposedBy);
    }

    private bool ValidateQuorum(IReadOnlyList<PolicyApprovalRecord> approvals)
    {
        var uniqueApprovers = approvals.Select(a => a.ApproverId).Distinct().Count();
        var approveCount = approvals.Count(a => a.Decision == "Approve");
        var rejectCount = approvals.Count(a => a.Decision == "Reject");

        if (approvals.Count - uniqueApprovers > 0) return false;
        if (rejectCount > 0) return false;

        return approveCount >= _defaultQuorum.RequiredApprovals;
    }

    private async Task<bool> VerifyArtifactIntegrityAsync(
        PolicyVersionRecord version, CancellationToken cancellationToken)
    {
        if (_artifactIntegrityVerifier is null)
            return true;

        return await _artifactIntegrityVerifier.VerifyAsync(
            version.ArtifactLocation!, version.ArtifactHash!, cancellationToken);
    }

    private string ComputeActivationHash(Guid policyId, int version)
    {
        var input = $"{policyId}:{version}:{_clock.UtcNowOffset:O}";
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

/// <summary>
/// Contract for artifact integrity verification.
/// Infrastructure adapter implements this (e.g., MinIO SHA-256 check).
/// </summary>
public interface IArtifactIntegrityVerifier
{
    Task<bool> VerifyAsync(string artifactLocation, string expectedHash, CancellationToken cancellationToken = default);
}
