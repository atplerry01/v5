using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Access;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.Access.Reducer;

public static class StreamAccessProjectionReducer
{
    public static StreamAccessReadModel Apply(StreamAccessReadModel state, StreamAccessGrantedEventSchema e) =>
        state with
        {
            AccessId = e.AggregateId,
            StreamId = e.StreamId,
            Mode = e.Mode,
            WindowStart = e.WindowStart,
            WindowEnd = e.WindowEnd,
            Token = e.Token,
            Status = "Granted",
            Reason = null,
            GrantedAt = e.GrantedAt,
            LastModifiedAt = e.GrantedAt
        };

    public static StreamAccessReadModel Apply(StreamAccessReadModel state, StreamAccessRestrictedEventSchema e) =>
        state with
        {
            AccessId = e.AggregateId,
            Status = "Restricted",
            Reason = e.Reason,
            LastModifiedAt = e.RestrictedAt
        };

    public static StreamAccessReadModel Apply(StreamAccessReadModel state, StreamAccessUnrestrictedEventSchema e) =>
        state with
        {
            AccessId = e.AggregateId,
            Status = "Granted",
            Reason = null,
            LastModifiedAt = e.UnrestrictedAt
        };

    public static StreamAccessReadModel Apply(StreamAccessReadModel state, StreamAccessRevokedEventSchema e) =>
        state with
        {
            AccessId = e.AggregateId,
            Status = "Revoked",
            Reason = e.Reason,
            LastModifiedAt = e.RevokedAt
        };

    public static StreamAccessReadModel Apply(StreamAccessReadModel state, StreamAccessExpiredEventSchema e) =>
        state with
        {
            AccessId = e.AggregateId,
            Status = "Expired",
            LastModifiedAt = e.ExpiredAt
        };
}
