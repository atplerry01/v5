namespace Whycespace.Shared.Contracts.Control.Observability.SystemTrace;

public sealed record SystemTraceReadModel
{
    public Guid SpanId { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public string OperationName { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public string? ParentSpanId { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? Status { get; init; }
}
