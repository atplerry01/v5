namespace Whyce.Shared.Contracts.Runtime;

/// <summary>
/// Thrown by the runtime workflow admission gate when in-flight
/// workflow executions exceed the declared per-workflow-name or
/// per-tenant ceiling.
///
/// phase1.5-S5.2.2 / KC-6 (WORKFLOW-ADMISSION-01): this is the typed
/// RETRYABLE REFUSAL path that closes K-R-05. It carries the
/// canonical bounded response class so the API edge can map it to
/// HTTP 503 + <c>Retry-After</c> via the dedicated
/// <c>WorkflowSaturatedExceptionHandler</c> seam — mirroring the
/// phase1-gate-api-edge precedent set by
/// <c>ConcurrencyConflictException</c>, the phase1.5-S5.2.1 / PC-2
/// precedent set by <c>PolicyEvaluationUnavailableException</c>, and
/// the phase1.5-S5.2.1 / PC-3 precedent set by
/// <c>OutboxSaturatedException</c>.
///
/// Workflow semantics are non-allowing by construction: no engine
/// step runs when this exception is thrown. The workflow is
/// *refused at the gate*, never partially executed. The caller is
/// expected to retry after the indicated interval.
/// </summary>
public sealed class WorkflowSaturatedException : Exception
{
    /// <summary>
    /// Workflow name (or partition key) that triggered the refusal.
    /// Surfaced on the problem-details payload for operator
    /// visibility.
    /// </summary>
    public string WorkflowName { get; }

    /// <summary>
    /// Partition that hit the ceiling: <c>"workflow"</c> when the
    /// per-workflow-name ceiling fired, <c>"tenant"</c> when the
    /// per-tenant ceiling fired.
    /// </summary>
    public string Partition { get; }

    /// <summary>
    /// Suggested retry delay in seconds. Surfaces as the HTTP
    /// <c>Retry-After</c> header. Sourced from
    /// <c>WorkflowOptions.RetryAfterSeconds</c>.
    /// </summary>
    public int RetryAfterSeconds { get; }

    public WorkflowSaturatedException(string workflowName, string partition, int retryAfterSeconds)
        : base($"Workflow '{workflowName}' is saturated on partition '{partition}'. " +
               "Refusing new execution. No bypass allowed.")
    {
        WorkflowName = workflowName;
        Partition = partition;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
