using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Replay;

namespace Whycespace.Projections.Content.Streaming.PlaybackConsumption.Replay.Reducer;

public static class ReplayProjectionReducer
{
    public static ReplayReadModel Apply(ReplayReadModel state, ReplayRequestedEventSchema e) =>
        state with
        {
            ReplayId = e.AggregateId,
            ArchiveId = e.ArchiveId,
            ViewerId = e.ViewerId,
            PositionMs = 0,
            Status = "Requested",
            RequestedAt = e.RequestedAt,
            LastModifiedAt = e.RequestedAt
        };

    public static ReplayReadModel Apply(ReplayReadModel state, ReplayStartedEventSchema e) =>
        state with { ReplayId = e.AggregateId, PositionMs = e.PositionMs, Status = "Active", LastModifiedAt = e.StartedAt };

    public static ReplayReadModel Apply(ReplayReadModel state, ReplayPausedEventSchema e) =>
        state with { ReplayId = e.AggregateId, PositionMs = e.PositionMs, Status = "Paused", LastModifiedAt = e.PausedAt };

    public static ReplayReadModel Apply(ReplayReadModel state, ReplayResumedEventSchema e) =>
        state with { ReplayId = e.AggregateId, Status = "Active", LastModifiedAt = e.ResumedAt };

    public static ReplayReadModel Apply(ReplayReadModel state, ReplayCompletedEventSchema e) =>
        state with { ReplayId = e.AggregateId, PositionMs = e.PositionMs, Status = "Completed", LastModifiedAt = e.CompletedAt };

    public static ReplayReadModel Apply(ReplayReadModel state, ReplayAbandonedEventSchema e) =>
        state with { ReplayId = e.AggregateId, Status = "Abandoned", LastModifiedAt = e.AbandonedAt };
}
