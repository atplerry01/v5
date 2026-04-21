using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Broadcast;

namespace Whycespace.Projections.Content.Streaming.LiveStreaming.Broadcast.Reducer;

public static class BroadcastProjectionReducer
{
    public static BroadcastReadModel Apply(BroadcastReadModel state, BroadcastCreatedEventSchema e) =>
        state with
        {
            BroadcastId = e.AggregateId,
            StreamId = e.StreamId,
            Status = "Created",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static BroadcastReadModel Apply(BroadcastReadModel state, BroadcastScheduledEventSchema e) =>
        state with
        {
            BroadcastId = e.AggregateId,
            Status = "Scheduled",
            ScheduledStart = e.ScheduledStart,
            ScheduledEnd = e.ScheduledEnd,
            LastModifiedAt = e.ScheduledAt
        };

    public static BroadcastReadModel Apply(BroadcastReadModel state, BroadcastStartedEventSchema e) =>
        state with
        {
            BroadcastId = e.AggregateId,
            Status = "Live",
            StartedAt = state.StartedAt ?? e.StartedAt,
            LastModifiedAt = e.StartedAt
        };

    public static BroadcastReadModel Apply(BroadcastReadModel state, BroadcastPausedEventSchema e) =>
        state with { BroadcastId = e.AggregateId, Status = "Paused", LastModifiedAt = e.PausedAt };

    public static BroadcastReadModel Apply(BroadcastReadModel state, BroadcastResumedEventSchema e) =>
        state with { BroadcastId = e.AggregateId, Status = "Live", LastModifiedAt = e.ResumedAt };

    public static BroadcastReadModel Apply(BroadcastReadModel state, BroadcastEndedEventSchema e) =>
        state with
        {
            BroadcastId = e.AggregateId,
            Status = "Ended",
            EndedAt = e.EndedAt,
            LastModifiedAt = e.EndedAt
        };

    public static BroadcastReadModel Apply(BroadcastReadModel state, BroadcastCancelledEventSchema e) =>
        state with
        {
            BroadcastId = e.AggregateId,
            Status = "Cancelled",
            CancellationReason = e.Reason,
            EndedAt = e.CancelledAt,
            LastModifiedAt = e.CancelledAt
        };
}
