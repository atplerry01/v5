namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Phase 8 B5 — read-only scan over the sanction projection used by
/// <c>SanctionExpirySchedulerWorker</c>. Returns every active sanction
/// whose projected ExpiresAt has reached or passed <paramref name="now"/>,
/// bounded by <paramref name="batchSize"/>.
///
/// <para>
/// <b>Fail-safe posture.</b> Implementations MUST catch transport / query
/// failures and return an empty list rather than propagating — a missed
/// scheduler iteration is a bounded delay that self-heals on the next
/// tick. This is DIFFERENT from <see cref="ILockStateQuery"/>'s
/// fail-closed posture: the scheduler is driving natural-expiry timing,
/// not gating command execution against a live safety rail.
/// </para>
///
/// <para>
/// <b>Determinism.</b> The query MUST NOT consult the DB's clock — the
/// scheduler passes <paramref name="now"/> as a parameter so the
/// candidate set is a pure function of (projection state, supplied
/// timestamp). This is what makes re-scan after restart replay-stable.
/// </para>
/// </summary>
public interface IExpirableSanctionQuery
{
    Task<IReadOnlyList<ExpirableSanctionCandidate>> QueryExpirableAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken = default);
}
