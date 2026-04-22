using Whycespace.Shared.Contracts.Control.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemTrace;

namespace Whycespace.Projections.Control.Observability.SystemTrace.Reducer;

public static class SystemTraceProjectionReducer
{
    public static SystemTraceReadModel Apply(SystemTraceReadModel state, SystemTraceSpanStartedEventSchema e) =>
        state with
        {
            SpanId        = e.AggregateId,
            TraceId       = e.TraceId,
            OperationName = e.OperationName,
            StartedAt     = e.StartedAt,
            ParentSpanId  = e.ParentSpanId
        };

    public static SystemTraceReadModel Apply(SystemTraceReadModel state, SystemTraceSpanCompletedEventSchema e) =>
        state with
        {
            CompletedAt = e.CompletedAt,
            Status      = e.Status
        };
}
