namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Projection-based, read-only policy read model.
/// Backed by Redis / projection store — NOT the write-side database.
/// Engines use this to query policy state without holding repositories or cache.
/// </summary>
public interface IPolicyReadModel
{
    // ── Policy queries ──────────────────────────────────────────

    Task<PolicyRecord?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyRecord>> GetPoliciesByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyRecord>> GetAllPoliciesAsync(CancellationToken cancellationToken = default);

    // ── Version queries ─────────────────────────────────────────

    Task<PolicyVersionRecord?> GetActiveVersionAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<PolicyVersionRecord?> GetVersionAsync(Guid policyId, int version, CancellationToken cancellationToken = default);
    Task<PolicyVersionRecord?> GetVersionByTimeAsync(Guid policyId, DateTimeOffset timestamp, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyVersionRecord>> GetVersionsByPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyActiveSnapshotRecord>> GetAllActiveSnapshotsAsync(CancellationToken cancellationToken = default);

    // ── Governance queries (read-only) ──────────────────────────

    Task<PolicyProposalRecord?> GetProposalAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyApprovalRecord>> GetApprovalsAsync(Guid proposalId, CancellationToken cancellationToken = default);
    Task<PolicyActivationRecord?> GetActivationByVersionAsync(Guid policyVersionId, CancellationToken cancellationToken = default);
}
