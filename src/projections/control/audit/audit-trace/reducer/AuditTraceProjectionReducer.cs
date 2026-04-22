using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditTrace;

namespace Whycespace.Projections.Control.Audit.AuditTrace.Reducer;

public static class AuditTraceProjectionReducer
{
    public static AuditTraceReadModel Apply(AuditTraceReadModel state, AuditTraceOpenedEventSchema e) =>
        state with
        {
            TraceId        = e.AggregateId,
            CorrelationId  = e.CorrelationId,
            OpenedAt       = e.OpenedAt,
            Status         = "Open",
            LinkedEventIds = []
        };

    public static AuditTraceReadModel Apply(AuditTraceReadModel state, AuditTraceEventLinkedEventSchema e) =>
        state with
        {
            LinkedEventIds = [..state.LinkedEventIds, e.AuditEventId]
        };

    public static AuditTraceReadModel Apply(AuditTraceReadModel state, AuditTraceClosedEventSchema e) =>
        state with
        {
            Status   = "Closed",
            ClosedAt = e.ClosedAt
        };
}
