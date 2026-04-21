using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Events.Content.Document.Governance.Retention;

namespace Whycespace.Projections.Content.Document.Governance.Retention.Reducer;

public static class RetentionProjectionReducer
{
    public static RetentionReadModel Apply(RetentionReadModel state, RetentionAppliedEventSchema e) =>
        state with
        {
            RetentionId = e.AggregateId,
            TargetId = e.TargetId,
            TargetKind = e.TargetKind,
            WindowAppliedAt = e.WindowAppliedAt,
            WindowExpiresAt = e.WindowExpiresAt,
            Reason = e.Reason,
            Status = "Applied",
            AppliedAt = e.AppliedAt,
            LastModifiedAt = e.AppliedAt
        };

    public static RetentionReadModel Apply(RetentionReadModel state, RetentionHoldPlacedEventSchema e) =>
        state with
        {
            RetentionId = e.AggregateId,
            Reason = e.Reason,
            Status = "Held",
            LastModifiedAt = e.PlacedAt
        };

    public static RetentionReadModel Apply(RetentionReadModel state, RetentionReleasedEventSchema e) =>
        state with
        {
            RetentionId = e.AggregateId,
            Status = "Released",
            LastModifiedAt = e.ReleasedAt
        };

    public static RetentionReadModel Apply(RetentionReadModel state, RetentionExpiredEventSchema e) =>
        state with
        {
            RetentionId = e.AggregateId,
            Status = "Expired",
            LastModifiedAt = e.ExpiredAt
        };

    public static RetentionReadModel Apply(RetentionReadModel state, RetentionMarkedEligibleForDestructionEventSchema e) =>
        state with
        {
            RetentionId = e.AggregateId,
            Status = "EligibleForDestruction",
            LastModifiedAt = e.MarkedAt
        };

    public static RetentionReadModel Apply(RetentionReadModel state, RetentionArchivedEventSchema e) =>
        state with
        {
            RetentionId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };
}
