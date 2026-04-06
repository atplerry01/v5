using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Observability;

public enum TraceSpanKind
{
    Command,
    Middleware,
    Workflow,
    Engine,
    Event
}

public sealed class TraceSpan
{
    private readonly IClock _clock;

    public required Guid SpanId { get; init; }
    public required string Name { get; init; }
    public required TraceSpanKind Kind { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public TimeSpan? Elapsed => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Dictionary<string, string> Tags { get; } = new();
    public List<TraceSpan> Children { get; } = new();

    public TraceSpan(IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;
    }

    public void Complete(bool success, string? errorMessage = null)
    {
        CompletedAt = _clock.UtcNowOffset;
        Success = success;
        ErrorMessage = errorMessage;
    }
}

public sealed class ExecutionTrace
{
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;

    public required Guid TraceId { get; init; }
    public required Guid CommandId { get; init; }
    public required string CorrelationId { get; init; }
    public required string ExecutionId { get; init; }
    public required string CommandType { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public TimeSpan? TotalElapsed => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    public bool Success { get; private set; }
    public List<TraceSpan> Spans { get; } = new();

    public ExecutionTrace(IClock? clock = null, IIdGenerator? idGenerator = null)
    {
        _clock = clock ?? SystemClock.Instance;
        _idGenerator = idGenerator ?? DefaultGuidGenerator.Instance;
    }

    public TraceSpan BeginSpan(string name, TraceSpanKind kind)
    {
        var span = new TraceSpan(_clock) { SpanId = _idGenerator.DeterministicGuid($"span:{TraceId}:{name}:{kind}"), Name = name, Kind = kind, StartedAt = _clock.UtcNowOffset };
        Spans.Add(span);
        return span;
    }

    public void Complete(bool success)
    {
        CompletedAt = _clock.UtcNowOffset;
        Success = success;
    }
}
