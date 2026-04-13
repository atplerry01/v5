namespace Whycespace.Shared.Contracts.Infrastructure.Persistence;

public interface IIdempotencyStore
{
    /// <summary>
    /// phase1.5-S5.2.2 / KC-2 (IDEMPOTENCY-COALESCE-01): atomic
    /// claim-or-detect-duplicate in a single DB round-trip. Returns
    /// <c>true</c> when this caller is the first to claim
    /// <paramref name="key"/> (the canonical "first-seen" path) and
    /// <c>false</c> when a prior claim already exists (the canonical
    /// "duplicate" path). Replaces the pre-KC-2 two-step shape
    /// <see cref="ExistsAsync"/> + <see cref="MarkAsync"/>, dropping
    /// per-command idempotency pool consumption from 2 acquisitions
    /// to 1 on the hot path.
    ///
    /// Failure-rollback semantics: when a claim is acquired but the
    /// inner pipeline subsequently fails, the caller MUST invoke
    /// <see cref="ReleaseAsync"/> to remove the claim row so that a
    /// retry of the same logical command is not permanently blocked.
    /// This preserves the pre-KC-2 "mark only on success" semantics
    /// of <c>IdempotencyMiddleware</c> exactly.
    /// </summary>
    // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): the hot-path
    // claim contract now consumes the request/host-shutdown CT so it
    // can reach the underlying ExecuteNonQueryAsync call in
    // PostgresIdempotencyStoreAdapter.
    Task<bool> TryClaimAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// phase1.5-S5.2.2 / KC-2: rollback for a claim acquired by
    /// <see cref="TryClaimAsync"/> when the inner pipeline failed.
    /// Idempotent — DELETE on a non-existent key is a no-op. Only
    /// invoked on the failure path; the happy path never calls this.
    /// </summary>
    // phase1.5-S5.2.3 / TC-5: rollback path also accepts CT for symmetry
    // with TryClaimAsync, even though it is only called on the failure
    // branch.
    Task ReleaseAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pre-KC-2 two-step API. Retained for interface stability and
    /// any consumer that has not yet migrated to
    /// <see cref="TryClaimAsync"/>. The runtime middleware no longer
    /// uses this method on the hot path.
    /// </summary>
    [System.Obsolete("phase1.5-S5.2.2 / KC-2: use TryClaimAsync instead. ExistsAsync + MarkAsync doubles per-command pool consumption.")]
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pre-KC-2 two-step API. See <see cref="ExistsAsync"/>.
    /// </summary>
    [System.Obsolete("phase1.5-S5.2.2 / KC-2: use TryClaimAsync instead. ExistsAsync + MarkAsync doubles per-command pool consumption.")]
    Task MarkAsync(string key, CancellationToken cancellationToken = default);
}
