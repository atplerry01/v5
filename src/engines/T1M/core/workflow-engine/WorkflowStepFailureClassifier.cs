namespace Whycespace.Engines.T1M.Core.WorkflowEngine;

/// <summary>
/// R3.A.5 / R-WORKFLOW-STEP-EXCEPTION-CLASSIFICATION-01 — maps step
/// exceptions to a retry decision using BCL exception types.
///
/// Layer constraint: the engine assembly (<c>Whycespace.Engines</c>)
/// MUST NOT reference the runtime assembly (dependency graph
/// discipline). This classifier therefore duplicates a minimal slice
/// of <c>RuntimeExceptionMapper</c>'s logic — matching BCL exception
/// types to a retry decision — rather than calling the mapper
/// directly. The policy remains aligned with <c>RuntimeFailureCategory</c>
/// conceptually (see inline comments below) but the implementation
/// is self-contained.
///
/// Classification policy (terminal — do NOT retry):
/// <list type="bullet">
///   <item><see cref="ArgumentException"/> and subclasses
///         (ArgumentNullException, ArgumentOutOfRangeException) —
///         maps to the RuntimeFailureCategory.ValidationFailed
///         concept. Retrying with the same input is identical.</item>
///   <item><see cref="UnauthorizedAccessException"/> and
///         <see cref="System.Security.SecurityException"/> — maps to
///         AuthorizationDenied. Retry with the same actor is identical.</item>
///   <item><see cref="InvalidOperationException"/> — maps to
///         InvalidState. Retry cannot fix state without external
///         mutation (which is R3.A.3 suspend territory).</item>
///   <item><see cref="NotSupportedException"/> — maps to InvalidState
///         (operation is structurally not possible).</item>
///   <item><see cref="OperationCanceledException"/> — maps to
///         Cancellation. Should not reach the engine's hard-failure
///         branch (CTS filters handle it), but listed so a
///         mis-classified OCE does not silently retry.</item>
/// </list>
///
/// Everything else is <see cref="WorkflowStepFailureClassification.Retryable"/>
/// — the conservative default preserves the R3.A.2 "retry everything
/// non-timeout" posture for unknown failures. Domain-specific terminal
/// exception types thrown by step handlers (e.g. <c>PolicyDeniedException</c>)
/// can be added to this switch as they ship; the default-retryable
/// posture ensures new unknown types do NOT silently fail-fast.
///
/// Stateless, pure function. Safe to call from any thread.
/// </summary>
public static class WorkflowStepFailureClassifier
{
    /// <summary>
    /// Classifies a workflow step exception as retryable or terminal.
    /// Uses pattern matching on BCL exception types — see the class
    /// XML-doc for the full policy.
    /// </summary>
    public static WorkflowStepFailureClassification Classify(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        return ex switch
        {
            // ValidationFailed family.
            ArgumentException => WorkflowStepFailureClassification.Terminal,

            // AuthorizationDenied family.
            UnauthorizedAccessException => WorkflowStepFailureClassification.Terminal,
            System.Security.SecurityException => WorkflowStepFailureClassification.Terminal,

            // InvalidState family.
            InvalidOperationException => WorkflowStepFailureClassification.Terminal,
            NotSupportedException => WorkflowStepFailureClassification.Terminal,

            // Cancellation — should not reach here (CTS filters handle it)
            // but defensive: never retry a cancellation.
            OperationCanceledException => WorkflowStepFailureClassification.Terminal,

            // Default: conservative retry. Covers transient DB / network /
            // dependency failures and any unknown exception types that may
            // be transient.
            _ => WorkflowStepFailureClassification.Retryable,
        };
    }

    /// <summary>
    /// Returns a short, low-cardinality category string suitable for
    /// use as a metric tag value. Uses the same BCL-type inspection as
    /// <see cref="Classify"/> and produces one of the canonical
    /// names aligned with <c>RuntimeFailureCategory</c> enum values.
    /// Low cardinality by construction — fixed set of ~6 values.
    /// </summary>
    public static string CategoryTag(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        return ex switch
        {
            ArgumentException => "ValidationFailed",
            UnauthorizedAccessException => "AuthorizationDenied",
            System.Security.SecurityException => "AuthorizationDenied",
            InvalidOperationException => "InvalidState",
            NotSupportedException => "InvalidState",
            OperationCanceledException => "Cancellation",
            _ => "ExecutionFailure",
        };
    }
}

/// <summary>
/// R3.A.5: retry decision outcome. Two values only — the finer
/// category tag is exposed via <see cref="WorkflowStepFailureClassifier.CategoryTag"/>
/// for metric tagging.
/// </summary>
public enum WorkflowStepFailureClassification
{
    /// <summary>Retrying may succeed; engine applies R3.A.2 backoff loop.</summary>
    Retryable = 0,

    /// <summary>Retrying cannot change the outcome; engine fast-fails.</summary>
    Terminal = 1,
}
