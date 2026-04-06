using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.Observability.Logging;

/// <summary>
/// T3I engine: produces structured log entries as read models.
/// Does NOT write to sinks — returns LogEntry for runtime to route.
/// </summary>
public sealed class LoggingEngine
{
    private readonly IClock _clock;

    public LoggingEngine(IClock clock)
    {
        _clock = clock;
    }

    public LogEntry LogCommand(string traceId, string correlationId, string commandType, string status)
    {
        return new LogEntry
        {
            Level = LogLevel.Info,
            TraceId = traceId,
            CorrelationId = correlationId,
            Category = "command",
            Message = $"Command '{commandType}' {status}",
            Timestamp = _clock.UtcNowOffset
        };
    }

    public LogEntry LogWorkflow(string traceId, string correlationId, string workflowName, string step, string status)
    {
        return new LogEntry
        {
            Level = LogLevel.Info,
            TraceId = traceId,
            CorrelationId = correlationId,
            Category = "workflow",
            Message = $"Workflow '{workflowName}' step '{step}' {status}",
            Timestamp = _clock.UtcNowOffset,
            Properties = new Dictionary<string, object> { ["workflow"] = workflowName, ["step"] = step }
        };
    }

    public LogEntry LogDecision(string traceId, string correlationId, string decisionType, bool allowed, string? reason)
    {
        return new LogEntry
        {
            Level = allowed ? LogLevel.Info : LogLevel.Warning,
            TraceId = traceId,
            CorrelationId = correlationId,
            Category = "decision",
            Message = $"Decision '{decisionType}': {(allowed ? "allowed" : "denied")}",
            Timestamp = _clock.UtcNowOffset,
            Properties = new Dictionary<string, object>
            {
                ["decisionType"] = decisionType,
                ["allowed"] = allowed,
                ["reason"] = reason ?? string.Empty
            }
        };
    }

    public LogEntry LogError(string traceId, string correlationId, string component, string error)
    {
        return new LogEntry
        {
            Level = LogLevel.Error,
            TraceId = traceId,
            CorrelationId = correlationId,
            Category = "error",
            Message = $"Error in '{component}': {error}",
            Timestamp = _clock.UtcNowOffset
        };
    }
}
