using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditQuery;

namespace Whycespace.Projections.Control.Audit.AuditQuery.Reducer;

public static class AuditQueryProjectionReducer
{
    public static AuditQueryReadModel Apply(AuditQueryReadModel state, AuditQueryIssuedEventSchema e) =>
        state with
        {
            QueryId           = e.AggregateId,
            IssuedBy          = e.IssuedBy,
            TimeRangeFrom     = e.TimeRangeFrom,
            TimeRangeTo       = e.TimeRangeTo,
            CorrelationFilter = e.CorrelationFilter,
            ActorFilter       = e.ActorFilter,
            Status            = "Issued"
        };

    public static AuditQueryReadModel Apply(AuditQueryReadModel state, AuditQueryCompletedEventSchema e) =>
        state with
        {
            Status      = "Completed",
            ResultCount = e.ResultCount
        };

    public static AuditQueryReadModel Apply(AuditQueryReadModel state, AuditQueryExpiredEventSchema e) =>
        state with { Status = "Expired" };
}
