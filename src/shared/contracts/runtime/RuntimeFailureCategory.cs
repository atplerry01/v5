namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Canonical runtime failure taxonomy. Every <see cref="CommandResult"/> rejection
/// carries one of these categories so downstream consumers (retry logic, audit,
/// operator UI) can classify without parsing free-text error strings.
///
/// See runtime-upgrade-plan.md §2 (R1) and spec §14 Canonical failure taxonomy.
/// R2 retry logic consumes this: Transient/Timeout/DependencyUnavailable are retry-eligible;
/// Permanent/PolicyDenied/AuthorizationDenied/ValidationFailed are not.
/// </summary>
public enum RuntimeFailureCategory
{
    Unknown = 0,

    ValidationFailed,
    AuthorizationDenied,
    PolicyDenied,
    /// <summary>
    /// R2.A.2 / POL-FAIL-CLASS-01 — policy evaluator could not produce
    /// a deterministic allow/deny (OPA outage, timeout). Retryable via
    /// <c>IRetryExecutor</c> with bounded attempts; on exhaustion the
    /// retry loop converts this to <see cref="PolicyDenied"/>.
    /// </summary>
    PolicyEvaluationDeferred,
    ConcurrencyConflict,
    PersistenceFailure,
    DependencyUnavailable,
    Timeout,
    Cancellation,
    PoisonMessage,
    RuntimeGuardRejection,
    InvalidState,
    ResourceExhausted,
    ExecutionFailure
}
