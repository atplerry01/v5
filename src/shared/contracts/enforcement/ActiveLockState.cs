namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Snapshot of the currently-active enforcement lock posture for a subject.
/// Produced by <see cref="ILockStateQuery"/> from the lock read model and
/// consumed by runtime middleware to reject command execution when an
/// active lock exists.
///
/// An active lock is a hard stop — all commands for the subject are
/// rejected until the lock is released. Absence of any active lock yields
/// <see cref="None"/>.
///
/// FAIL-CLOSED HARDENING: When the lock state cannot be verified due to
/// infrastructure failure, the query returns <see cref="Unavailable"/>
/// rather than <see cref="None"/>. The middleware MUST treat Unavailable
/// as a block — no command executes when lock state is unknown.
/// </summary>
public sealed record ActiveLockState(bool IsLocked, bool IsUnavailable, string? Scope, string? Reason)
{
    public static readonly ActiveLockState None = new(false, false, null, null);
    public static readonly ActiveLockState Unavailable = new(false, true, null, "lock_state_unavailable");
}
