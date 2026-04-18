namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Phase 8 B5 — read-only scan over the lock projection used by
/// <c>SystemLockExpirySchedulerWorker</c>. Returns every currently-locked
/// row whose projected ExpiresAt has reached or passed <paramref name="now"/>,
/// bounded by <paramref name="batchSize"/>. Suspended, Unlocked, and
/// already-Expired rows are excluded — the status='Locked' filter is the
/// authoritative "timer is running" signal because <c>LockAggregate.Suspend</c>
/// transitions the status away from Locked.
///
/// <para>
/// Fail-safe posture and determinism requirements mirror
/// <see cref="IExpirableSanctionQuery"/>.
/// </para>
/// </summary>
public interface IExpirableLockQuery
{
    Task<IReadOnlyList<ExpirableLockCandidate>> QueryExpirableAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken = default);
}
