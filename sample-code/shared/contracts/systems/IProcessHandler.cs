using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Shared.Contracts.Systems;

/// <summary>
/// Downstream process handler contract.
/// Platform calls downstream via this interface.
/// Downstream interprets business intent, selects workflow, and routes to WSS.
/// </summary>
public interface IProcessHandler
{
    string CommandPrefix { get; }
    bool CanHandle(string commandType);
    Task<IntentResult> HandleAsync(ProcessCommand command, CancellationToken cancellationToken = default);
}

public sealed record ProcessCommand
{
    public required Guid CommandId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required string CorrelationId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string? AggregateId { get; init; }
    public string? WhyceId { get; init; }
    public string? PolicyId { get; init; }
}

/// <summary>
/// Registry that resolves the correct downstream handler for a command type.
/// Keeps resolution logic out of the platform adapter (thin pass-through).
/// </summary>
public interface IProcessHandlerRegistry
{
    IProcessHandler Resolve(string commandType);
}

public sealed record ProcessResult
{
    public required bool Success { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public static ProcessResult Ok(object? data = null) => new() { Success = true, Data = data };
    public static ProcessResult Fail(string error, string? code = null) => new() { Success = false, ErrorMessage = error, ErrorCode = code };
}
