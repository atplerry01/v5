namespace Whycespace.Shared.Contracts.Systems.Intent;

/// <summary>
/// Systems layer intent dispatcher contract.
/// Systems compose and dispatch intents — never execute directly.
/// Runtime layer provides the implementation that bridges to execution.
/// </summary>
public interface ISystemIntentDispatcher
{
    Task<IntentResult> DispatchAsync(ExecuteCommandIntent intent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an intent dispatch.
/// Maps 1:1 with runtime execution outcome but decouples systems from runtime result types.
/// </summary>
public sealed record IntentResult
{
    public required Guid CommandId { get; init; }
    public required bool Success { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public static IntentResult Ok(Guid commandId, object? data = null) => new()
    {
        CommandId = commandId,
        Success = true,
        Data = data
    };

    public static IntentResult Fail(Guid commandId, string errorMessage, string? errorCode = null) => new()
    {
        CommandId = commandId,
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}
