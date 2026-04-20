using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Contracts;

/// <summary>
/// Runtime execution result. Returned by the RuntimeControlPlane after
/// command execution through the full pipeline.
/// Contains execution outcome, emitted events, and chain anchoring data.
/// </summary>
public sealed record RuntimeResult
{
    public required bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public RuntimeFailureCategory? FailureCategory { get; init; }
    public object? Output { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];
    public string? ChainBlockId { get; init; }
    public string? PolicyDecisionHash { get; init; }
    public string? ExecutionId { get; init; }

    public bool IsDuplicate { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? PreviousExecutionId { get; init; }

    public static RuntimeResult Success(
        IReadOnlyList<object> events,
        object? output = null,
        string? chainBlockId = null,
        string? policyDecisionHash = null,
        string? executionId = null,
        string? idempotencyKey = null) =>
        new()
        {
            IsSuccess = true,
            EmittedEvents = events,
            Output = output,
            ChainBlockId = chainBlockId,
            PolicyDecisionHash = policyDecisionHash,
            ExecutionId = executionId,
            IdempotencyKey = idempotencyKey
        };

    public static RuntimeResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };

    public static RuntimeResult Failure(string error, RuntimeFailureCategory category) =>
        new() { IsSuccess = false, Error = error, FailureCategory = category };

    /// <summary>
    /// Canonical duplicate-response shape (spec §8). A replay of a previously-processed
    /// idempotency key returns the original outcome with <see cref="IsDuplicate"/> = true
    /// and <see cref="PreviousExecutionId"/> pointing at the first execution.
    /// Emitted events / chain block / policy decision hash are copied from the original
    /// so downstream consumers see a stable reply for the same key.
    /// </summary>
    public static RuntimeResult AlreadyProcessed(RuntimeResult previous, string idempotencyKey) =>
        new()
        {
            IsSuccess = previous.IsSuccess,
            Error = previous.Error,
            FailureCategory = previous.FailureCategory,
            Output = previous.Output,
            EmittedEvents = previous.EmittedEvents,
            ChainBlockId = previous.ChainBlockId,
            PolicyDecisionHash = previous.PolicyDecisionHash,
            ExecutionId = previous.ExecutionId,
            IsDuplicate = true,
            IdempotencyKey = idempotencyKey,
            PreviousExecutionId = previous.ExecutionId
        };
}
