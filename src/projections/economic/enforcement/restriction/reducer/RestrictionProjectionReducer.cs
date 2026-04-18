using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Restriction;

namespace Whycespace.Projections.Economic.Enforcement.Restriction.Reducer;

public static class RestrictionProjectionReducer
{
    public static RestrictionReadModel Apply(RestrictionReadModel state, RestrictionAppliedEventSchema e) =>
        state with
        {
            RestrictionId = e.AggregateId,
            SubjectId = e.SubjectId,
            Scope = e.Scope,
            Reason = e.Reason,
            Status = "Applied",
            IsActive = true,
            AppliedAt = e.AppliedAt,
            RemovedAt = null,
            RemovalReason = string.Empty,
            LastUpdatedAt = e.AppliedAt,
            // Phase 7 B4 / T7.6 — cause-coupling projection. V1 events
            // carry Cause=null; leave the fields empty in that case so
            // the projection mirrors the aggregate (which synthesises a
            // Legacy/Manual cause on replay but does not re-stamp the
            // event).
            CauseKind = e.Cause?.Kind ?? string.Empty,
            CauseReferenceId = e.Cause?.CauseReferenceId,
            CauseDetail = e.Cause?.Detail ?? string.Empty,
        };

    public static RestrictionReadModel Apply(RestrictionReadModel state, RestrictionUpdatedEventSchema e) =>
        state with
        {
            RestrictionId = e.AggregateId,
            Scope = e.NewScope,
            Reason = e.NewReason,
            LastUpdatedAt = e.UpdatedAt,
        };

    public static RestrictionReadModel Apply(RestrictionReadModel state, RestrictionRemovedEventSchema e) =>
        state with
        {
            RestrictionId = e.AggregateId,
            Status = "Removed",
            IsActive = false,
            RemovedAt = e.RemovedAt,
            RemovalReason = e.RemovalReason,
            LastUpdatedAt = e.RemovedAt,
        };

    // Phase 7 B4 / T7.7 — suspend / resume lifecycle projection. IsActive
    // flips to false during suspension (middleware consumers key off it)
    // but the Applied-time scope/reason/cause remain intact so Resume
    // restores the exact pre-suspension state without invention.

    public static RestrictionReadModel Apply(RestrictionReadModel state, RestrictionSuspendedEventSchema e) =>
        state with
        {
            RestrictionId = e.AggregateId,
            Status = "Suspended",
            IsActive = false,
            SuspendedAt = e.SuspendedAt,
            SuspensionCauseKind = e.SuspensionCause.Kind,
            SuspensionCauseReferenceId = e.SuspensionCause.CauseReferenceId,
            SuspensionCauseDetail = e.SuspensionCause.Detail,
            LastUpdatedAt = e.SuspendedAt,
        };

    public static RestrictionReadModel Apply(RestrictionReadModel state, RestrictionResumedEventSchema e) =>
        state with
        {
            RestrictionId = e.AggregateId,
            Status = "Applied",
            IsActive = true,
            ResumedAt = e.ResumedAt,
            LastUpdatedAt = e.ResumedAt,
        };
}
