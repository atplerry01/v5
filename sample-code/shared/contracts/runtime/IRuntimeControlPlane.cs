namespace Whycespace.Shared.Contracts.Runtime;

public interface IRuntimeControlPlane
{
    Task<RuntimeCommandResult> ExecuteAsync(RuntimeCommandEnvelope envelope, CancellationToken cancellationToken = default);
}

public sealed record RuntimeCommandEnvelope
{
    public required Guid CommandId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public required string CorrelationId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? AggregateId { get; init; }
    public string? CausationId { get; init; }
    public string? WhyceId { get; init; }
    public string? PolicyId { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}

public sealed record RuntimeCommandResult
{
    public required Guid CommandId { get; init; }
    public required bool Success { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public static RuntimeCommandResult Ok(Guid commandId, object? data = null) => new()
    {
        CommandId = commandId,
        Success = true,
        Data = data
    };

    public static RuntimeCommandResult Fail(Guid commandId, string errorMessage, string? errorCode = null) => new()
    {
        CommandId = commandId,
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}
