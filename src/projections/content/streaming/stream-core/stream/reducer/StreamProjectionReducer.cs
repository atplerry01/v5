using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Stream;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Stream.Reducer;

public static class StreamProjectionReducer
{
    public static StreamReadModel Apply(StreamReadModel state, StreamCreatedEventSchema e) =>
        state with
        {
            StreamId = e.AggregateId,
            Mode = e.Mode,
            Type = e.Type,
            Status = "Created",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static StreamReadModel Apply(StreamReadModel state, StreamActivatedEventSchema e) =>
        state with
        {
            StreamId = e.AggregateId,
            Status = "Active",
            StartedAt = state.StartedAt ?? e.ActivatedAt,
            LastModifiedAt = e.ActivatedAt
        };

    public static StreamReadModel Apply(StreamReadModel state, StreamPausedEventSchema e) =>
        state with { StreamId = e.AggregateId, Status = "Paused", LastModifiedAt = e.PausedAt };

    public static StreamReadModel Apply(StreamReadModel state, StreamResumedEventSchema e) =>
        state with { StreamId = e.AggregateId, Status = "Active", LastModifiedAt = e.ResumedAt };

    public static StreamReadModel Apply(StreamReadModel state, StreamEndedEventSchema e) =>
        state with { StreamId = e.AggregateId, Status = "Ended", EndedAt = e.EndedAt, LastModifiedAt = e.EndedAt };

    public static StreamReadModel Apply(StreamReadModel state, StreamArchivedEventSchema e) =>
        state with { StreamId = e.AggregateId, Status = "Archived", LastModifiedAt = e.ArchivedAt };
}
