namespace Whycespace.Shared.Contracts.Observability;

public interface ILogService
{
    void LogCommand(string traceId, string correlationId, string commandType, string status);
    void LogWorkflow(string traceId, string correlationId, string workflowName, string step, string status);
    void LogDecision(string traceId, string correlationId, string decisionType, bool allowed, string? reason);
    void LogError(string traceId, string correlationId, string component, string error);
}

public sealed record LogEntry
{
    public required LogLevel Level { get; init; }
    public required string TraceId { get; init; }
    public required string CorrelationId { get; init; }
    public required string Category { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public IReadOnlyDictionary<string, object>? Properties { get; init; }
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public interface ILogSink
{
    void Write(LogEntry entry);
}
