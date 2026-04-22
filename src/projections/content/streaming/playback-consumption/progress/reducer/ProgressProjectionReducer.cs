using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Progress;

namespace Whycespace.Projections.Content.Streaming.PlaybackConsumption.Progress.Reducer;

public static class ProgressProjectionReducer
{
    public static ProgressReadModel Apply(ProgressReadModel state, ProgressTrackedEventSchema e) =>
        state with
        {
            ProgressId = e.AggregateId,
            SessionId = e.SessionId,
            PositionMs = e.PositionMs,
            Status = "Tracking",
            TrackedAt = e.TrackedAt,
            LastModifiedAt = e.TrackedAt
        };

    public static ProgressReadModel Apply(ProgressReadModel state, PlaybackPositionUpdatedEventSchema e) =>
        state with { ProgressId = e.AggregateId, PositionMs = e.PositionMs, LastModifiedAt = e.UpdatedAt };

    public static ProgressReadModel Apply(ProgressReadModel state, PlaybackPausedEventSchema e) =>
        state with { ProgressId = e.AggregateId, PositionMs = e.PositionMs, Status = "Paused", LastModifiedAt = e.PausedAt };

    public static ProgressReadModel Apply(ProgressReadModel state, PlaybackResumedEventSchema e) =>
        state with { ProgressId = e.AggregateId, Status = "Tracking", LastModifiedAt = e.ResumedAt };

    public static ProgressReadModel Apply(ProgressReadModel state, ProgressTerminatedEventSchema e) =>
        state with { ProgressId = e.AggregateId, Status = "Terminated", LastModifiedAt = e.TerminatedAt };
}
