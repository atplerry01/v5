namespace Whycespace.Shared.Contracts.Engine;

public sealed record WorkflowExecutionResult
{
    public bool IsSuccess { get; init; }

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-01 — workflow is durably paused awaiting
    /// an external approval signal. Suspension is not a failure: the
    /// dispatching command succeeded; the workflow is waiting. Set
    /// together with <see cref="IsSuccess"/>=true so existing dispatcher
    /// branches treating IsSuccess as "dispatch OK" continue to route
    /// the emitted events through persist → chain → outbox.
    /// </summary>
    public bool IsSuspended { get; init; }

    public object? Output { get; init; }
    public string? FailedStep { get; init; }
    public string? Error { get; init; }

    /// <summary>R3.A.6 — step at which the workflow suspended awaiting approval.</summary>
    public string? SuspendedStepName { get; init; }

    /// <summary>R3.A.6 — canonical <c>human_approval[:signal]</c> carrier recorded on the Suspended event.</summary>
    public string? SuspendedSignal { get; init; }

    public IReadOnlyList<object> EmittedEvents { get; init; } = [];

    public static WorkflowExecutionResult Success(object? output = null, IReadOnlyList<object>? events = null) =>
        new() { IsSuccess = true, Output = output, EmittedEvents = events ?? [] };

    public static WorkflowExecutionResult Failure(string failedStep, string error) =>
        new() { IsSuccess = false, FailedStep = failedStep, Error = error };

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-01 — workflow suspended pending a
    /// human-approval decision. Emitted events include the
    /// <c>WorkflowExecutionSuspendedEvent</c> plus any step-accumulated
    /// events up to the suspend point. The dispatcher persists these
    /// and returns a successful CommandResult; no workflow re-entry
    /// occurs until <c>ApproveWorkflowCommand</c> /
    /// <c>RejectWorkflowCommand</c> is dispatched.
    /// </summary>
    public static WorkflowExecutionResult Suspended(
        string stepName,
        string signal,
        IReadOnlyList<object>? events = null) =>
        new()
        {
            IsSuccess = true,
            IsSuspended = true,
            SuspendedStepName = stepName,
            SuspendedSignal = signal,
            EmittedEvents = events ?? []
        };
}
