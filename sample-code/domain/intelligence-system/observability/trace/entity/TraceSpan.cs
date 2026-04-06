namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

/// <summary>
/// Represents a single span within a distributed trace.
/// Models the structural relationship between operations across services.
/// </summary>
public sealed class TraceSpan
{
    public TraceId TraceId { get; }
    public SpanId SpanId { get; }
    public SpanId ParentSpanId { get; }
    public string OperationName { get; }
    public DateTimeOffset StartTime { get; }
    public DateTimeOffset? EndTime { get; private set; }
    public SpanStatus Status { get; private set; }

    public TraceSpan(
        TraceId traceId,
        SpanId spanId,
        SpanId parentSpanId,
        string operationName,
        DateTimeOffset startTime)
    {
        TraceId = traceId;
        SpanId = spanId;
        ParentSpanId = parentSpanId;
        OperationName = operationName;
        StartTime = startTime;
        Status = SpanStatus.Unset;
    }

    public void Complete(DateTimeOffset endTime, SpanStatus status)
    {
        EndTime = endTime;
        Status = status;
    }
}
