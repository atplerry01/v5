namespace Whycespace.Runtime.Command;

public sealed record CommandResult
{
    public required Guid CommandId { get; init; }
    public required bool Success { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }

    public static CommandResult Ok(Guid commandId, object? data = null, DateTimeOffset? completedAt = null) => new()
    {
        CommandId = commandId,
        Success = true,
        Data = data,
        CompletedAt = completedAt ?? default
    };

    public static CommandResult Fail(Guid commandId, string errorMessage, string? errorCode = null, DateTimeOffset? completedAt = null) => new()
    {
        CommandId = commandId,
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode,
        CompletedAt = completedAt ?? default
    };
}
