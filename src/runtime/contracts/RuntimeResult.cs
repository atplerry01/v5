namespace Whyce.Runtime.Contracts;

/// <summary>
/// Runtime execution result. Returned by the RuntimeControlPlane after
/// command execution through the full pipeline.
/// Contains execution outcome, emitted events, and chain anchoring data.
/// </summary>
public sealed record RuntimeResult
{
    public required bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public object? Output { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];
    public string? ChainBlockId { get; init; }
    public string? PolicyDecisionHash { get; init; }
    public string? ExecutionId { get; init; }

    public static RuntimeResult Success(
        IReadOnlyList<object> events,
        object? output = null,
        string? chainBlockId = null,
        string? policyDecisionHash = null,
        string? executionId = null) =>
        new()
        {
            IsSuccess = true,
            EmittedEvents = events,
            Output = output,
            ChainBlockId = chainBlockId,
            PolicyDecisionHash = policyDecisionHash,
            ExecutionId = executionId
        };

    public static RuntimeResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
