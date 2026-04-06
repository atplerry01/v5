using Whycespace.Runtime.Command;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Simulation;

public sealed class SimulationContext
{
    private readonly IClock _clock;

    public Guid SimulationId { get; init; }
    public required CommandEnvelope Envelope { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public List<RuntimeEvent> CapturedEvents { get; } = new();
    public List<TraceSpan> CapturedSpans { get; } = new();
    public List<string> ExecutionLog { get; } = new();

    public SimulationContext(IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;
    }

    public void CaptureEvent(RuntimeEvent @event)
    {
        CapturedEvents.Add(@event);
        Log($"Event captured: {@event.EventType} [{@event.EventId}]");
    }

    public void CaptureSpan(TraceSpan span)
    {
        CapturedSpans.Add(span);
    }

    public void Log(string message)
    {
        ExecutionLog.Add($"[{_clock.UtcNowOffset:HH:mm:ss.fff}] {message}");
    }

    public void Complete()
    {
        CompletedAt = _clock.UtcNowOffset;
    }
}
