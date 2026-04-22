using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.IngestSession;

namespace Whycespace.Projections.Content.Streaming.LiveStreaming.IngestSession.Reducer;

public static class IngestSessionProjectionReducer
{
    public static IngestSessionReadModel Apply(IngestSessionReadModel state, IngestSessionAuthenticatedEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            BroadcastId = e.BroadcastId,
            Endpoint = e.Endpoint,
            Status = "Authenticated",
            FailureReason = null,
            AuthenticatedAt = e.AuthenticatedAt,
            LastModifiedAt = e.AuthenticatedAt
        };

    public static IngestSessionReadModel Apply(IngestSessionReadModel state, IngestStreamingStartedEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Streaming", LastModifiedAt = e.StartedAt };

    public static IngestSessionReadModel Apply(IngestSessionReadModel state, IngestSessionStalledEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Stalled", LastModifiedAt = e.StalledAt };

    public static IngestSessionReadModel Apply(IngestSessionReadModel state, IngestSessionResumedEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Streaming", LastModifiedAt = e.ResumedAt };

    public static IngestSessionReadModel Apply(IngestSessionReadModel state, IngestSessionEndedEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Ended", LastModifiedAt = e.EndedAt };

    public static IngestSessionReadModel Apply(IngestSessionReadModel state, IngestSessionFailedEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Failed", FailureReason = e.FailureReason, LastModifiedAt = e.FailedAt };
}
