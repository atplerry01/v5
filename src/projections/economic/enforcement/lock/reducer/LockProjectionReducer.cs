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
            LastUpdatedAt = e.LockedAt
        };

    public static LockReadModel Apply(LockReadModel state, SystemUnlockedEventSchema e) =>
        state with
        {
            LockId = e.AggregateId,
            Status = "Unlocked",
            IsActive = false,
            UnlockedAt = e.UnlockedAt,
            UnlockReason = e.UnlockReason,
            LastUpdatedAt = e.UnlockedAt
        };
}
