using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Lock;

namespace Whycespace.Projections.Economic.Enforcement.Lock.Reducer;

public static class LockProjectionReducer
{
    public static LockReadModel Apply(LockReadModel state, SystemLockedEventSchema e) =>
        state with
        {
            LockId = e.AggregateId,
            SubjectId = e.SubjectId,
            Scope = e.Scope,
            Reason = e.Reason,
            Status = "Locked",
            IsActive = true,
            LockedAt = e.LockedAt,
            UnlockedAt = null,
            UnlockReason = string.Empty,
            LastUpdatedAt = e.LockedAt,
            // Phase 7 B4 / T7.6 + T7.9 — cause-coupling + natural-expiry.
            CauseKind = e.Cause?.Kind ?? string.Empty,
            CauseReferenceId = e.Cause?.CauseReferenceId,
            CauseDetail = e.Cause?.Detail ?? string.Empty,
            ExpiresAt = e.ExpiresAt,
        };

    public static LockReadModel Apply(LockReadModel state, SystemUnlockedEventSchema e) =>
        state with
        {
            LockId = e.AggregateId,
            Status = "Unlocked",
            IsActive = false,
            UnlockedAt = e.UnlockedAt,
            UnlockReason = e.UnlockReason,
            LastUpdatedAt = e.UnlockedAt,
        };

    // Phase 7 B4 / T7.8 — suspend / resume. IsActive flips to false
    // during suspension; Lock-time Scope/Reason/Cause/ExpiresAt stay
    // intact so Resume restores exactly.

    public static LockReadModel Apply(LockReadModel state, SystemLockSuspendedEventSchema e) =>
        state with
        {
            LockId = e.AggregateId,
            Status = "Suspended",
            IsActive = false,
            SuspendedAt = e.SuspendedAt,
            SuspensionCauseKind = e.SuspensionCause.Kind,
            SuspensionCauseReferenceId = e.SuspensionCause.CauseReferenceId,
            SuspensionCauseDetail = e.SuspensionCause.Detail,
            LastUpdatedAt = e.SuspendedAt,
        };

    public static LockReadModel Apply(LockReadModel state, SystemLockResumedEventSchema e) =>
        state with
        {
            LockId = e.AggregateId,
            Status = "Locked",
            IsActive = true,
            ResumedAt = e.ResumedAt,
            LastUpdatedAt = e.ResumedAt,
        };

    // Phase 7 B4 / T7.9 — natural terminal expiry. Distinct from Unlock
    // (explicit release).

    public static LockReadModel Apply(LockReadModel state, SystemLockExpiredEventSchema e) =>
        state with
        {
            LockId = e.AggregateId,
            Status = "Expired",
            IsActive = false,
            ExpiredAt = e.ExpiredAt,
            LastUpdatedAt = e.ExpiredAt,
        };
}
