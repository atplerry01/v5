using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Session;

namespace Whycespace.Projections.Content.Streaming.PlaybackConsumption.Session.Reducer;

public static class SessionProjectionReducer
{
    public static SessionReadModel Apply(SessionReadModel state, SessionOpenedEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            StreamId = e.StreamId,
            Status = "Opened",
            OpenedAt = e.OpenedAt,
            ExpiresAt = e.ExpiresAt,
            LastModifiedAt = e.OpenedAt
        };

    public static SessionReadModel Apply(SessionReadModel state, SessionActivatedEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Active", LastModifiedAt = e.ActivatedAt };

    public static SessionReadModel Apply(SessionReadModel state, SessionSuspendedEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Suspended", LastModifiedAt = e.SuspendedAt };

    public static SessionReadModel Apply(SessionReadModel state, SessionResumedEventSchema e) =>
        state with { SessionId = e.AggregateId, Status = "Active", LastModifiedAt = e.ResumedAt };

    public static SessionReadModel Apply(SessionReadModel state, SessionClosedEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            Status = "Closed",
            TerminationReason = e.Reason,
            ClosedAt = e.ClosedAt,
            LastModifiedAt = e.ClosedAt
        };

    public static SessionReadModel Apply(SessionReadModel state, SessionFailedEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            Status = "Failed",
            TerminationReason = e.Reason,
            ClosedAt = e.FailedAt,
            LastModifiedAt = e.FailedAt
        };

    public static SessionReadModel Apply(SessionReadModel state, SessionExpiredEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            Status = "Expired",
            ClosedAt = e.ExpiredAt,
            LastModifiedAt = e.ExpiredAt
        };
}
