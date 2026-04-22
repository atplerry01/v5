using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Moderation;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.Moderation.Reducer;

public static class ModerationProjectionReducer
{
    public static ModerationReadModel Apply(ModerationReadModel state, StreamFlaggedEventSchema e) =>
        state with
        {
            ModerationId = e.AggregateId,
            TargetId = e.TargetId,
            FlagReason = e.FlagReason,
            Status = "Flagged",
            FlaggedAt = e.FlaggedAt,
            LastModifiedAt = e.FlaggedAt
        };

    public static ModerationReadModel Apply(ModerationReadModel state, ModerationAssignedEventSchema e) =>
        state with
        {
            ModerationId = e.AggregateId,
            ModeratorId = e.ModeratorId,
            Status = "InReview",
            LastModifiedAt = e.AssignedAt
        };

    public static ModerationReadModel Apply(ModerationReadModel state, ModerationDecidedEventSchema e) =>
        state with
        {
            ModerationId = e.AggregateId,
            Decision = e.Decision,
            Rationale = e.Rationale,
            Status = "Decided",
            LastModifiedAt = e.DecidedAt
        };

    public static ModerationReadModel Apply(ModerationReadModel state, ModerationOverturnedEventSchema e) =>
        state with
        {
            ModerationId = e.AggregateId,
            Rationale = e.Rationale,
            Status = "Overturned",
            LastModifiedAt = e.OverturnedAt
        };
}
