namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Short-lived in-memory cache of enforcement decisions populated directly
/// from the event stream (NOT from projections). Provides a faster
/// enforcement path that closes the projection lag window.
///
/// PROJECTION LAG PROTECTION: Enforcement projections are eventually
/// consistent. Between event emission and projection materialization, a
/// command could execute despite an active violation/lock. This cache
/// is populated synchronously by the enforcement detection handler and
/// ViolationToEscalation handler, so it reflects enforcement state
/// within milliseconds of the event rather than seconds-to-minutes of
/// projection lag.
///
/// Semantics:
///   • TryGetLock returns the cached lock state if a recent lock event
///     was processed; null if no cached entry (fall through to projection).
///   • TryGetViolation returns the cached violation posture if a recent
///     violation event was processed; null if no cached entry.
///   • Entries expire after <see cref="DefaultTtl"/> to prevent stale
///     state from blocking indefinitely — the projection is authoritative
///     for long-term state.
///
/// Thread-safe. Non-blocking. Never throws on infrastructure failure.
/// </summary>
public interface IEnforcementDecisionCache
{
    static readonly TimeSpan DefaultTtl = TimeSpan.FromSeconds(30);

    void RecordLock(Guid subjectId, ActiveLockState state);
    void RecordViolation(Guid subjectId, ActiveViolationState state);

    /// <summary>
    /// Phase 2 — authoritative restriction cache entry. Populated directly by
    /// <c>ApplyRestrictionHandler</c> so the very next command in the same
    /// process sees the restriction without waiting on projection lag.
    /// Cleared by <see cref="ClearRestriction"/> on remove.
    /// </summary>
    void RecordRestriction(Guid subjectId, ActiveRestrictionState state);

    /// <summary>
    /// Phase 2 — explicit removal (no soft-TTL race) called on restriction
    /// remove so a later command does not see a stale "restricted" entry.
    /// </summary>
    void ClearRestriction(Guid subjectId);

    ActiveLockState? TryGetLock(Guid subjectId);
    ActiveViolationState? TryGetViolation(Guid subjectId);
    ActiveRestrictionState? TryGetRestriction(Guid subjectId);
}
