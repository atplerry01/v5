namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Thrown by <c>T1MWorkflowEngine</c> when a workflow step or the
/// overall workflow execution exceeds its declared timeout from
/// <c>WorkflowOptions.PerStepTimeoutMs</c> /
/// <c>WorkflowOptions.MaxExecutionMs</c>.
///
/// phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): typed RETRYABLE
/// REFUSAL counterpart to <c>WorkflowSaturatedException</c>. Together
/// they form the canonical workflow refusal family — saturation refuses
/// at the admission gate, timeout refuses inside the engine. Both map
/// to HTTP 503 + <c>Retry-After</c> at the API edge via dedicated
/// exception handlers, mirroring the chain-anchor refusal family
/// (<c>ChainAnchorWaitTimeoutException</c> + <c>ChainAnchorUnavailableException</c>).
///
/// Caller-driven cancellation (an <see cref="OperationCanceledException"/>
/// caused by the request CT) is NOT wrapped — it propagates as-is so
/// the host pipeline observes shutdown semantics directly. The typed
/// exception is reserved for the *declared* timeout case.
/// </summary>
public sealed class WorkflowTimeoutException : Exception
{
    /// <summary>
    /// Low-cardinality kind tag. One of: "step", "execution".
    /// Surfaced on the problem-details payload for operator visibility.
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// Step name when <see cref="Kind"/> is "step"; null otherwise.
    /// </summary>
    public string? StepName { get; }

    /// <summary>
    /// Configured timeout, in milliseconds, that was exceeded. Sourced
    /// from <c>WorkflowOptions.PerStepTimeoutMs</c> or
    /// <c>WorkflowOptions.MaxExecutionMs</c>.
    /// </summary>
    public int TimeoutMs { get; }

    /// <summary>
    /// Suggested retry delay in seconds. Surfaces as the HTTP
    /// <c>Retry-After</c> header. Sourced from
    /// <c>WorkflowOptions.RetryAfterSeconds</c>.
    /// </summary>
    public int RetryAfterSeconds { get; }

    public WorkflowTimeoutException(
        string kind,
        string? stepName,
        int timeoutMs,
        int retryAfterSeconds,
        string message)
        : base(message)
    {
        Kind = kind;
        StepName = stepName;
        TimeoutMs = timeoutMs;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
