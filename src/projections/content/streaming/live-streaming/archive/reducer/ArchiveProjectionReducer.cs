using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Archive;

namespace Whycespace.Projections.Content.Streaming.LiveStreaming.Archive.Reducer;

public static class ArchiveProjectionReducer
{
    public static ArchiveReadModel Apply(ArchiveReadModel state, ArchiveStartedEventSchema e) =>
        state with
        {
            ArchiveId = e.AggregateId,
            StreamId = e.StreamId,
            SessionId = e.SessionId,
            Status = "Started",
            StartedAt = e.StartedAt,
            LastModifiedAt = e.StartedAt
        };

    public static ArchiveReadModel Apply(ArchiveReadModel state, ArchiveCompletedEventSchema e) =>
        state with
        {
            ArchiveId = e.AggregateId,
            Status = "Completed",
            OutputId = e.OutputId,
            CompletedAt = e.CompletedAt,
            LastModifiedAt = e.CompletedAt
        };

    public static ArchiveReadModel Apply(ArchiveReadModel state, ArchiveFailedEventSchema e) =>
        state with
        {
            ArchiveId = e.AggregateId,
            Status = "Failed",
            FailureReason = e.Reason,
            CompletedAt = e.FailedAt,
            LastModifiedAt = e.FailedAt
        };

    public static ArchiveReadModel Apply(ArchiveReadModel state, ArchiveFinalizedEventSchema e) =>
        state with
        {
            ArchiveId = e.AggregateId,
            Status = "Finalized",
            FinalizedAt = e.FinalizedAt,
            LastModifiedAt = e.FinalizedAt
        };

    public static ArchiveReadModel Apply(ArchiveReadModel state, ArchiveArchivedEventSchema e) =>
        state with
        {
            ArchiveId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };
}
