namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R2.A.1 — canonical retry primitive. Executes a (possibly failure-prone)
/// operation with bounded attempts, deterministic backoff-with-jitter, and
/// category-driven retry eligibility.
///
/// Replay determinism (R-RETRY-DET-01): implementations MUST source time
/// from <see cref="Whycespace.Shared.Kernel.Domain.IClock"/> and jitter
/// from <see cref="Whycespace.Shared.Kernel.Domain.IRandomProvider"/>
/// seeded from the <see cref="RetryOperationContext.OperationId"/>.
/// No wall-clock reads, no hidden entropy. Two calls with the same
/// context + inner operation outcomes MUST produce identical
/// <see cref="RetryResult{T}"/>.
///
/// Boundedness (R-RETRY-CAP-01): every call terminates in at most
/// <see cref="RetryPolicy.MaxAttempts"/> attempts regardless of failure
/// category. After exhaustion the executor returns
/// <see cref="RetryOutcome.Exhausted"/> — never loops further.
///
/// Category eligibility (R-RETRY-CAT-01): retry decisions are a pure
/// function of <see cref="RuntimeFailureCategory"/>. String-based
/// permanent-failure heuristics are deprecated.
///
/// Evidence (R-RETRY-EVIDENCE-01): the returned <see cref="RetryResult{T}"/>
/// carries one <see cref="RetryAttemptRecord"/> per attempt, suitable for
/// feeding <c>RetryAttemptedEvent</c> / <c>RetryExhaustedEvent</c> audit
/// emissions in the calling engine/adapter.
/// </summary>
public interface IRetryExecutor
{
    Task<RetryResult<T>> ExecuteAsync<T>(
        RetryOperationContext context,
        Func<int, CancellationToken, Task<RetryStepResult<T>>> operation,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Per-call context for <see cref="IRetryExecutor.ExecuteAsync{T}"/>.
/// <see cref="OperationId"/> seeds the jitter RNG — same operation id
/// reproduces the same jitter sequence on replay.
/// </summary>
public sealed record RetryOperationContext
{
    /// <summary>
    /// Deterministic seed source for jitter. Typical shape:
    /// <c>$"{correlationId}:{operationName}"</c>. MUST NOT include
    /// wall-clock ticks, random values, or any non-replayable input.
    /// </summary>
    public required string OperationId { get; init; }

    public required RetryPolicy Policy { get; init; }

    /// <summary>
    /// Optional short label for observability (metric tags, log fields).
    /// Not used in jitter derivation.
    /// </summary>
    public string? OperationName { get; init; }
}

/// <summary>
/// Outcome of a single attempt supplied to the retry executor. Callers
/// return this instead of throwing so the executor can classify via
/// <see cref="FailureCategory"/> without inspecting exception types.
/// </summary>
public sealed record RetryStepResult<T>
{
    public required bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public RuntimeFailureCategory? FailureCategory { get; init; }
    public string? Error { get; init; }

    public static RetryStepResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    public static RetryStepResult<T> Failure(RuntimeFailureCategory category, string error) =>
        new() { IsSuccess = false, FailureCategory = category, Error = error };
}

/// <summary>
/// Final outcome of a retry sequence. <see cref="Attempts"/> is the
/// full evidence trail for audit emission (R-RETRY-EVIDENCE-01).
/// </summary>
public sealed record RetryResult<T>
{
    public required RetryOutcome Outcome { get; init; }
    public T? Value { get; init; }
    public int AttemptsMade { get; init; }
    public RuntimeFailureCategory? FinalFailureCategory { get; init; }
    public string? FinalError { get; init; }
    public IReadOnlyList<RetryAttemptRecord> Attempts { get; init; } = [];
}

/// <summary>
/// One record per attempt. Every field is replay-deterministic:
/// timestamps from <c>IClock</c>, delay from the canonical backoff
/// formula + seeded jitter.
/// </summary>
public sealed record RetryAttemptRecord(
    int AttemptNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    TimeSpan DelayBeforeAttempt,
    bool IsSuccess,
    RuntimeFailureCategory? FailureCategory,
    string? Error);

/// <summary>
/// Retryability classification for <see cref="RuntimeFailureCategory"/>.
/// Implementations use the canonical predicate
/// <see cref="RetryEligibility.IsRetryable"/>; this record is exposed as
/// contract so tests and policy code can assert the mapping.
/// </summary>
public static class RetryEligibility
{
    /// <summary>
    /// R-RETRY-CAT-01 canonical mapping. Retry eligibility MUST NOT be
    /// decided on error-string content.
    /// </summary>
    public static bool IsRetryable(RuntimeFailureCategory? category) => category switch
    {
        RuntimeFailureCategory.Timeout => true,
        RuntimeFailureCategory.DependencyUnavailable => true,
        RuntimeFailureCategory.ConcurrencyConflict => true,
        RuntimeFailureCategory.ResourceExhausted => true,
        RuntimeFailureCategory.ExecutionFailure => true,
        RuntimeFailureCategory.PersistenceFailure => true,
        RuntimeFailureCategory.PolicyEvaluationDeferred => true,

        RuntimeFailureCategory.AuthorizationDenied => false,
        RuntimeFailureCategory.PolicyDenied => false,
        RuntimeFailureCategory.ValidationFailed => false,
        RuntimeFailureCategory.RuntimeGuardRejection => false,
        RuntimeFailureCategory.InvalidState => false,
        RuntimeFailureCategory.PoisonMessage => false,
        RuntimeFailureCategory.Cancellation => false,

        RuntimeFailureCategory.Unknown => false,
        null => false,
        _ => false
    };
}
