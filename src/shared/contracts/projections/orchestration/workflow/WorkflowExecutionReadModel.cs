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
