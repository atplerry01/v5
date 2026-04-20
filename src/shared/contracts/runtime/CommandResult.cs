namespace Whycespace.Shared.Contracts.Runtime;

public sealed record CommandResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    /// <summary>
    /// R1 §14 — canonical runtime failure taxonomy. Populated on every failure
    /// so downstream retry logic (R2) and operator UI can classify without
    /// parsing <see cref="Error"/>. Null on success.
    /// </summary>
    public RuntimeFailureCategory? FailureCategory { get; init; }

    /// <summary>
    /// R1 §5 — sub-classification when <see cref="FailureCategory"/> is
    /// <see cref="RuntimeFailureCategory.ValidationFailed"/>. Null otherwise.
    /// </summary>
    public ValidationFailureCategory? ValidationCategory { get; init; }

    /// <summary>
    /// R1 §10 — sub-classification when <see cref="FailureCategory"/> is
    /// <see cref="RuntimeFailureCategory.PersistenceFailure"/>. Null otherwise.
    /// </summary>
    public PersistenceFailureCategory? PersistenceCategory { get; init; }

    public object? Output { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];
    public bool EventsRequirePersistence { get; init; }

    /// <summary>
    /// R1 §8 — canonical duplicate-response shape. True when the runtime
    /// detected this command's idempotency key has already been processed
    /// and the original outcome is being replayed.
    /// </summary>
    public bool IsDuplicate { get; init; }

    /// <summary>
    /// R1 §8 — idempotency key surfaced on the result so callers / audit
    /// can correlate the current reply with the original execution.
    /// Null when idempotency was not evaluated.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// R1 §8 — when <see cref="IsDuplicate"/> is true, points at the original
    /// execution's CommandId so reconciliation can link duplicate hits to
    /// their source. Null otherwise.
    /// </summary>
    public Guid? PreviousExecutionCommandId { get; init; }

    /// <summary>
    /// phase1-gate-S7: correlation id used for EventStore append, WhyceChain
    /// anchor, and Outbox enqueue. Surfacing it on the result lets API callers
    /// trace a single request from the HTTP response all the way to the chain
    /// row, fixing VERDICT.md Drift #5.
    /// </summary>
    public Guid CorrelationId { get; init; }

    /// <summary>
    /// Optional audit emission carried alongside the result. Persisted by the
    /// runtime control plane to a dedicated stream BEFORE domain events,
    /// independent of IsSuccess. Used by PolicyMiddleware to record allow/deny
    /// decisions to the constitutional policy decision stream.
    /// PolicyDecisionHash on the context is still mandatory — audit emission
    /// records governed decisions, never bypasses.
    /// </summary>
    public AuditEmission? AuditEmission { get; init; }

    /// <summary>
    /// Phase 8 B6 — structured deny reason surfaced by the policy evaluator
    /// (OPA rego <c>deny_reason</c> set, <c>reasons[]</c> array, or legacy
    /// single-string <c>reason</c> / <c>denial_reason</c>). Populated ONLY
    /// on the policy-denied branch; <c>null</c> on allow-path and on
    /// non-policy failures. Coexists with <see cref="Error"/> — the Error
    /// string remains the free-form, caller-facing message; this field
    /// carries the machine-readable token(s) for audit correlation and
    /// downstream rego rule attribution.
    /// </summary>
    public string? PolicyDenyReason { get; init; }

    public static CommandResult Success(IReadOnlyList<object> events, object? output = null, bool eventsRequirePersistence = true) =>
        new() { IsSuccess = true, EmittedEvents = events, Output = output, EventsRequirePersistence = eventsRequirePersistence };

    public static CommandResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };

    /// <summary>R1 §14 — failure with canonical category for retry/audit classification.</summary>
    public static CommandResult Failure(string error, RuntimeFailureCategory category) =>
        new() { IsSuccess = false, Error = error, FailureCategory = category };

    /// <summary>R1 §5 — validation failure with sub-classification.</summary>
    public static CommandResult ValidationFailure(string error, ValidationFailureCategory validationCategory) =>
        new()
        {
            IsSuccess = false,
            Error = error,
            FailureCategory = RuntimeFailureCategory.ValidationFailed,
            ValidationCategory = validationCategory
        };

    /// <summary>R1 §10 — persistence failure with sub-classification.</summary>
    public static CommandResult PersistenceFailure(string error, PersistenceFailureCategory persistenceCategory) =>
        new()
        {
            IsSuccess = false,
            Error = error,
            FailureCategory = RuntimeFailureCategory.PersistenceFailure,
            PersistenceCategory = persistenceCategory
        };

    /// <summary>
    /// R1 §8 — canonical duplicate-response. A replay of a previously-processed
    /// idempotency key returns the original outcome with <see cref="IsDuplicate"/> = true
    /// and <see cref="PreviousExecutionCommandId"/> pointing at the first execution.
    /// Events / audit emission are copied so downstream consumers see a stable reply.
    /// </summary>
    public static CommandResult AlreadyProcessed(
        CommandResult previous,
        string idempotencyKey,
        Guid previousExecutionCommandId) =>
        new()
        {
            IsSuccess = previous.IsSuccess,
            Error = previous.Error,
            FailureCategory = previous.FailureCategory,
            ValidationCategory = previous.ValidationCategory,
            PersistenceCategory = previous.PersistenceCategory,
            Output = previous.Output,
            EmittedEvents = previous.EmittedEvents,
            EventsRequirePersistence = false,
            CorrelationId = previous.CorrelationId,
            AuditEmission = previous.AuditEmission,
            PolicyDenyReason = previous.PolicyDenyReason,
            IsDuplicate = true,
            IdempotencyKey = idempotencyKey,
            PreviousExecutionCommandId = previousExecutionCommandId
        };
}
