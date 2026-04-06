using System.Collections.Concurrent;
using Whycespace.Runtime.Command;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Observability;

public sealed class TraceManager
{
    private readonly ConcurrentDictionary<Guid, ExecutionTrace> _activeTraces = new();
    private readonly ConcurrentQueue<ExecutionTrace> _completedTraces = new();
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;

    public TraceManager(IClock? clock = null, IIdGenerator? idGenerator = null)
    {
        _clock = clock ?? SystemClock.Instance;
        _idGenerator = idGenerator ?? DefaultGuidGenerator.Instance;
    }

    public ExecutionTrace BeginTrace(CommandEnvelope envelope, string? executionId = null)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var trace = new ExecutionTrace(_clock, _idGenerator)
        {
            TraceId = _idGenerator.DeterministicGuid($"trace:{envelope.CommandId}:{envelope.CorrelationId}"),
            CommandId = envelope.CommandId,
            CorrelationId = envelope.CorrelationId,
            ExecutionId = executionId ?? envelope.CommandId.ToString(),
            CommandType = envelope.CommandType,
            StartedAt = _clock.UtcNowOffset
        };

        _activeTraces[envelope.CommandId] = trace;
        return trace;
    }

    public ExecutionTrace? GetActiveTrace(Guid commandId)
    {
        _activeTraces.TryGetValue(commandId, out var trace);
        return trace;
    }

    public void CompleteTrace(Guid commandId, bool success)
    {
        if (_activeTraces.TryRemove(commandId, out var trace))
        {
            trace.Complete(success);
            _completedTraces.Enqueue(trace);
        }
    }

    public TraceSpan? BeginSpan(Guid commandId, string name, TraceSpanKind kind)
    {
        var trace = GetActiveTrace(commandId);
        return trace?.BeginSpan(name, kind);
    }

    public IReadOnlyList<ExecutionTrace> GetCompletedTraces()
    {
        return [.. _completedTraces];
    }

    public int ActiveCount => _activeTraces.Count;
    public int CompletedCount => _completedTraces.Count;
}
