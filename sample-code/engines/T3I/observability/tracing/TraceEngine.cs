using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.Observability.Tracing;

/// <summary>
/// T3I engine: produces trace entries as read models.
/// Does NOT write to stores — returns TraceEntry for runtime to route.
/// </summary>
public sealed class TraceEngine
{
    private readonly IClock _clock;
    private readonly IIdGenerator _idGen;

    public TraceEngine(IClock clock, IIdGenerator idGen)
    {
        _clock = clock;
        _idGen = idGen;
    }

    public (TraceContext Context, TraceEntry Entry) StartTrace(string correlationId, string commandType)
    {
        var context = new TraceContext
        {
            TraceId = _idGen.DeterministicGuid($"Trace:{correlationId}:{commandType}").ToString("N"),
            CorrelationId = correlationId,
            CommandType = commandType,
            StartedAt = _clock.UtcNowOffset
        };

        var entry = new TraceEntry
        {
            TraceId = context.TraceId,
            CorrelationId = correlationId,
            Phase = "command.received",
            Component = commandType,
            Timestamp = context.StartedAt
        };

        return (context, entry);
    }

    public TraceEntry RecordWorkflowStep(TraceContext context, string workflowName, string stepName)
    {
        return new TraceEntry
        {
            TraceId = context.TraceId,
            CorrelationId = context.CorrelationId,
            Phase = "workflow.step",
            Component = $"{workflowName}.{stepName}",
            Timestamp = _clock.UtcNowOffset
        };
    }

    public TraceEntry RecordEngineInvocation(TraceContext context, string engineName, TimeSpan duration, bool success)
    {
        return new TraceEntry
        {
            TraceId = context.TraceId,
            CorrelationId = context.CorrelationId,
            Phase = "engine.invocation",
            Component = engineName,
            Timestamp = _clock.UtcNowOffset,
            DurationMs = (long)duration.TotalMilliseconds,
            Success = success
        };
    }

    public TraceEntry RecordEvent(TraceContext context, string eventType)
    {
        return new TraceEntry
        {
            TraceId = context.TraceId,
            CorrelationId = context.CorrelationId,
            Phase = "event.emitted",
            Component = eventType,
            Timestamp = _clock.UtcNowOffset
        };
    }

    public TraceEntry CompleteTrace(TraceContext context, bool success)
    {
        return new TraceEntry
        {
            TraceId = context.TraceId,
            CorrelationId = context.CorrelationId,
            Phase = "command.completed",
            Component = context.CommandType,
            Timestamp = _clock.UtcNowOffset,
            DurationMs = (long)(_clock.UtcNowOffset - context.StartedAt).TotalMilliseconds,
            Success = success
        };
    }
}
