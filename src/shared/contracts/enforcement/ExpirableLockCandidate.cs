namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Phase 8 B5 — minimal projection row the system-lock expiry scheduler
/// needs to compose a deterministic ExpireSystemLockCommand. Shape parity
/// with <see cref="ExpirableSanctionCandidate"/>: authoritative aggregate
/// id + projected ExpiresAt, nothing more.
///
/// <para>
/// Suspended locks are excluded at the query boundary (not by a field on
/// this record) — the Lock aggregate transitions Status away from 'Locked'
/// on Suspend, so a status='Locked' filter on the projection is the
/// authoritative "timer is running" signal.
/// </para>
/// </summary>
public sealed record ExpirableLockCandidate(
    Guid LockId,
    DateTimeOffset ExpiresAt);
