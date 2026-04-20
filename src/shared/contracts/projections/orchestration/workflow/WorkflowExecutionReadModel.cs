namespace Whycespace.Shared.Contracts.Projections.Orchestration.Workflow;

public sealed record WorkflowExecutionReadModel
{
    public required Guid WorkflowExecutionId { get; init; }
    public required string WorkflowName { get; init; }
    public int CurrentStepIndex { get; init; }
    public string ExecutionHash { get; init; } = string.Empty;
    public string Status { get; init; } = "Running";
    public string? FailedStepName { get; init; }
    public string? FailureReason { get; init; }
    public object? Payload { get; init; }

    // R3.A.6 / R-WF-APPROVAL-PROJ-01 — derived approval-context fields.
    // Canonical lifecycle Status is preserved (Running/Suspended/Cancelled/…);
    // approval semantics live here so "Rejected" stays approval context and
    // never overloads the authoritative lifecycle Status enum.

    /// <summary>
    /// R3.A.6 — approval lifecycle state derived from canonical
    /// human_approval[_granted|_rejected] carrier prefixes on the
    /// latest Suspended / Resumed / Cancelled event. One of
    /// <c>"AwaitingApproval"</c>, <c>"Granted"</c>, <c>"Rejected"</c>,
    /// or null (non-approval suspend/cancel, or workflow never entered
    /// an approval wait-state).
    /// </summary>
    public string? ApprovalState { get; init; }

    /// <summary>
    /// R3.A.6 — signal suffix parsed from the <c>human_approval:{signal}</c>
    /// carrier on the latest Suspended event. Surface value — not
    /// authoritative. Null when the signal was the bare
    /// <c>human_approval</c> prefix or the workflow is not in an
    /// approval wait-state.
    /// </summary>
    public string? ApprovalSignal { get; init; }

    /// <summary>
    /// R3.A.6 — rationale suffix parsed from the
    /// <c>human_approval_rejected:{signal}:{actor}:{rationale}</c>
    /// carrier on the Cancelled event when rejected. Null until a
    /// rejection lands or on non-rejection transitions. Observability
    /// / UX surface — the authoritative rationale lives in the command
    /// audit envelope.
    /// </summary>
    public string? ApprovalDecision { get; init; }

    // phase1-gate-projection-hardening: StepOutputs is now exposed as
    // IReadOnlyDictionary so callers cannot mutate stored state via the
    // returned reference. The handler must construct a new dictionary
    // before applying `with`.
    public IReadOnlyDictionary<string, object?> StepOutputs { get; init; }
        = new Dictionary<string, object?>();

    // phase1-gate-projection-hardening: per-event idempotency token, mirrors
    // the Todo projection's last_event_id pattern. The handler short-circuits
    // when the incoming envelope.EventId matches this value, making same-event
    // replay a no-op. Combined with H7a per-aggregate Kafka ordering, this
    // closes the duplicate-replay vector without needing aggregate version
    // plumbing through the envelope.
    public Guid? LastEventId { get; init; }
}
