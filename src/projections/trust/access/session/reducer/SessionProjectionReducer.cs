using Whycespace.Shared.Contracts.Events.Trust.Access.Session;
using Whycespace.Shared.Contracts.Trust.Access.Session;

namespace Whycespace.Projections.Trust.Access.Session.Reducer;

public static class SessionProjectionReducer
{
    public static SessionReadModel Apply(SessionReadModel state, SessionOpenedEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            IdentityReference = e.IdentityReference,
            SessionContext = e.SessionContext,
            Status = "Open",
            OpenedAt = e.OpenedAt,
            LastUpdatedAt = e.OpenedAt
        };

    public static SessionReadModel Apply(SessionReadModel state, SessionExpiredEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            Status = "Expired",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static SessionReadModel Apply(SessionReadModel state, SessionTerminatedEventSchema e) =>
        state with
        {
            SessionId = e.AggregateId,
            Status = "Terminated",
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
}
