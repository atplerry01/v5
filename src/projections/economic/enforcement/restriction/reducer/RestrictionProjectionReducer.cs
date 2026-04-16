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
            LastUpdatedAt = e.AppliedAt
        };

    public static RestrictionReadModel Apply(RestrictionReadModel state, RestrictionUpdatedEventSchema e) =>
        state with
        {
            RestrictionId = e.AggregateId,
            Scope = e.NewScope,
            Reason = e.NewReason,
            LastUpdatedAt = e.UpdatedAt
        };

    public static RestrictionReadModel Apply(RestrictionReadModel state, RestrictionRemovedEventSchema e) =>
        state with
        {
            RestrictionId = e.AggregateId,
            Status = "Removed",
            IsActive = false,
            RemovedAt = e.RemovedAt,
            RemovalReason = e.RemovalReason,
            LastUpdatedAt = e.RemovedAt
        };
}
