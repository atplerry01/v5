namespace Whycespace.Runtime.Reliability;

/// <summary>
/// Handles recovery of in-flight workflows interrupted by failover.
/// Determines which workflows need replay, compensation, or restart.
/// Runtime orchestration only — no business logic.
/// </summary>
public sealed class WorkflowRecoveryHandler
{
    /// <summary>
    /// Analyzes interrupted workflows and produces recovery actions.
    /// </summary>
    public WorkflowRecoveryPlan Analyze(IReadOnlyList<InterruptedWorkflow> workflows)
    {
        ArgumentNullException.ThrowIfNull(workflows);

        var actions = new List<RecoveryAction>();

        foreach (var wf in workflows)
        {
            var action = wf.State switch
            {
                WorkflowInterruptState.Pending => new RecoveryAction(
                    wf.WorkflowId, RecoveryActionType.Restart, "Workflow was pending — safe to restart"),

                WorkflowInterruptState.InProgress when wf.IsIdempotent => new RecoveryAction(
                    wf.WorkflowId, RecoveryActionType.Replay, "Idempotent workflow — safe to replay from last checkpoint"),

                WorkflowInterruptState.InProgress when !wf.IsIdempotent => new RecoveryAction(
                    wf.WorkflowId, RecoveryActionType.Compensate, "Non-idempotent workflow — must compensate completed steps"),

                WorkflowInterruptState.Compensating => new RecoveryAction(
                    wf.WorkflowId, RecoveryActionType.ContinueCompensation, "Compensation was in progress — continue"),

                _ => new RecoveryAction(
                    wf.WorkflowId, RecoveryActionType.ManualReview, "Unknown state — requires manual review")
            };

            actions.Add(action);
        }

        return new WorkflowRecoveryPlan(actions);
    }
}

public sealed record InterruptedWorkflow(
    Guid WorkflowId,
    WorkflowInterruptState State,
    bool IsIdempotent,
    int CompletedSteps,
    int TotalSteps);

public enum WorkflowInterruptState
{
    Pending,
    InProgress,
    Compensating,
    Unknown
}

public sealed record RecoveryAction(
    Guid WorkflowId,
    RecoveryActionType ActionType,
    string Reason);

public enum RecoveryActionType
{
    Restart,
    Replay,
    Compensate,
    ContinueCompensation,
    ManualReview
}

public sealed record WorkflowRecoveryPlan(IReadOnlyList<RecoveryAction> Actions)
{
    public int RestartCount => Actions.Count(a => a.ActionType == RecoveryActionType.Restart);
    public int ReplayCount => Actions.Count(a => a.ActionType == RecoveryActionType.Replay);
    public int CompensateCount => Actions.Count(a => a.ActionType is RecoveryActionType.Compensate or RecoveryActionType.ContinueCompensation);
    public int ManualReviewCount => Actions.Count(a => a.ActionType == RecoveryActionType.ManualReview);
}
