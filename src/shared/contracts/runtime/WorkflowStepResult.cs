namespace Whycespace.Shared.Contracts.Runtime;

public sealed record WorkflowStepResult
{
    public bool IsSuccess { get; init; }

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-01 — step is yielding control to an
    /// external human-approval wait-state. The engine halts execution,
    /// emits <c>WorkflowExecutionSuspendedEvent</c> with the canonical
    /// <c>human_approval</c>-prefixed <see cref="ApprovalSignal"/> as
    /// Reason, and returns a Suspended <c>WorkflowExecutionResult</c>.
    /// Resume via <c>ApproveWorkflowCommand</c> / <c>RejectWorkflowCommand</c>.
    ///
    /// Orthogonal to <see cref="IsSuccess"/>: suspension is neither a
    /// success nor a failure outcome — it is a durable pause.
    /// </summary>
    public bool IsAwaitingApproval { get; init; }

    public string? Error { get; init; }

    /// <summary>
    /// R3.A.6 — canonical <c>human_approval[:signal[:…]]</c> carrier
    /// supplied when <see cref="IsAwaitingApproval"/> is true. Verbatim
    /// forwarded to <c>WorkflowLifecycleEventFactory.Suspended(...)</c>
    /// as the event Reason. Must start with the
    /// <c>human_approval</c> prefix per R-WF-APPROVAL-01.
    /// </summary>
    public string? ApprovalSignal { get; init; }

    /// <summary>
    /// R3.A.6 — optional override for the StepName recorded on the
    /// Suspended event. Defaults to the engine-inferred current step
    /// name when null.
    /// </summary>
    public string? ApprovalStepName { get; init; }

    public object? Output { get; init; }
    public IReadOnlyList<object> Events { get; init; } = [];

    public static WorkflowStepResult Success(object? output = null, IReadOnlyList<object>? events = null) =>
        new() { IsSuccess = true, Output = output, Events = events ?? [] };

    public static WorkflowStepResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-01 — step is yielding to a human-approval
    /// wait-state. <paramref name="approvalSignal"/> MUST start with
    /// the canonical <c>human_approval</c> prefix (optionally followed
    /// by a <c>:signal</c> suffix identifying the approval). The engine
    /// emits <c>WorkflowExecutionSuspendedEvent</c> with this value as
    /// Reason and halts; approval dispatch resumes or cancels.
    /// </summary>
    public static WorkflowStepResult AwaitingApproval(
        string approvalSignal, string? approvalStepName = null) =>
        new()
        {
            IsAwaitingApproval = true,
            ApprovalSignal = approvalSignal,
            ApprovalStepName = approvalStepName
        };
}
