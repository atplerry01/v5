namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyVersionRepository
{
    Task CreateVersionAsync(PolicyVersionRecord record, CancellationToken cancellationToken = default);
    Task<PolicyVersionRecord?> GetActiveVersionAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<PolicyVersionRecord?> GetVersionAsync(Guid policyId, int version, CancellationToken cancellationToken = default);
    Task<PolicyVersionRecord?> GetVersionByTimeAsync(Guid policyId, DateTimeOffset timestamp, CancellationToken cancellationToken = default);
    Task LockVersionAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task ActivateVersionAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<Guid> ActivateVersionSafeAsync(Guid versionId, Guid activatedBy, string activationHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyVersionRecord>> GetVersionsByPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<PolicyActiveSnapshotRecord?> GetActiveSnapshotAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyActiveSnapshotRecord>> GetAllActiveSnapshotsAsync(CancellationToken cancellationToken = default);
}

public sealed record PolicyActiveSnapshotRecord(
    Guid PolicyId,
    Guid VersionId,
    int Version,
    string? ArtifactLocation,
    string? ArtifactHash,
    DateTimeOffset ActivatedAt);

public sealed record PolicyVersionRecord(
    Guid Id,
    Guid PolicyId,
    int Version,
    string Status,
    string? ArtifactLocation,
    string? ArtifactHash,
    bool IsLocked,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ActivatedAt,
    long? ActivationSequence = null);
