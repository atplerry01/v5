using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// Event-sourced workflow execution aggregate. Sole authority for workflow
/// execution state under WBSM v3.5 H9 — there is no in-memory state store and
/// no external truth. All state is derived from the event stream via
/// <see cref="LoadFromHistory"/>.
///
/// Lifecycle invariants (enforced inside the aggregate):
/// <list type="bullet">
///   <item>Cannot complete before started (status must be Running)</item>
///   <item>Cannot record a step after completed</item>
///   <item>Cannot resume unless previously failed</item>
///   <item>Cannot skip steps — step index must match the next expected slot</item>
/// </list>
/// </summary>
public sealed class WorkflowExecutionAggregate : AggregateRoot
{
    public WorkflowExecutionId Id { get; private set; }
    private string _workflowName = string.Empty;
    private int _currentStepIndex;
    private string _executionHash = string.Empty;
    private WorkflowExecutionStatus _status = WorkflowExecutionStatus.NotStarted;
    private readonly List<string> _completedSteps = [];
    private string? _failedStepName;
    private string? _failureReason;

    public string WorkflowName => _workflowName;
    public int CurrentStepIndex => _currentStepIndex;
    public string ExecutionHash => _executionHash;
    public WorkflowExecutionStatus Status => _status;
    public IReadOnlyList<string> CompletedSteps => _completedSteps;
    public string? FailedStepName => _failedStepName;
    public string? FailureReason => _failureReason;

    private WorkflowExecutionAggregate() { }

    public static WorkflowExecutionAggregate Start(WorkflowExecutionId id, string workflowName)
    {
        Guard.Against(string.IsNullOrWhiteSpace(workflowName), WorkflowExecutionErrors.WorkflowNameRequired);

        var aggregate = new WorkflowExecutionAggregate { Id = id };
        aggregate.RaiseDomainEvent(new WorkflowExecutionStartedEvent(new AggregateId(id.Value), workflowName));
        return aggregate;
    }

    public void CompleteStep(int stepIndex, string stepName, string executionHash)
    {
        Guard.Against(string.IsNullOrWhiteSpace(stepName), WorkflowExecutionErrors.StepNameRequired);
        Guard.Against(_status == WorkflowExecutionStatus.NotStarted, WorkflowExecutionErrors.NotRunning);
        Guard.Against(_status == WorkflowExecutionStatus.Completed, WorkflowExecutionErrors.CannotStepAfterCompleted);
        Guard.Against(_status != WorkflowExecutionStatus.Running, WorkflowExecutionErrors.NotRunning);
        Guard.Against(stepIndex != _completedSteps.Count, WorkflowExecutionErrors.CannotSkipSteps);

        RaiseDomainEvent(new WorkflowStepCompletedEvent(
            new AggregateId(Id.Value), stepIndex, stepName, executionHash));
    }

    public void Complete(string executionHash)
    {
        Guard.Against(_status == WorkflowExecutionStatus.NotStarted, WorkflowExecutionErrors.CannotCompleteBeforeStarted);
        Guard.Against(_status != WorkflowExecutionStatus.Running, WorkflowExecutionErrors.NotRunning);

        RaiseDomainEvent(new WorkflowExecutionCompletedEvent(new AggregateId(Id.Value), executionHash));
    }

    public void Fail(string failedStepName, string reason)
    {
        Guard.Against(string.IsNullOrWhiteSpace(failedStepName), WorkflowExecutionErrors.StepNameRequired);
        Guard.Against(_status == WorkflowExecutionStatus.NotStarted, WorkflowExecutionErrors.NotRunning);
        Guard.Against(_status == WorkflowExecutionStatus.Completed, WorkflowExecutionErrors.CannotStepAfterCompleted);

        RaiseDomainEvent(new WorkflowExecutionFailedEvent(new AggregateId(Id.Value), failedStepName, reason));
    }

    // phase1.6-S1.2 (E-LIFECYCLE-FACTORY-CALL-SITE-01): the previous public
    // Resume() command method has been removed. WorkflowExecutionResumedEvent
    // is now constructed exclusively by WorkflowLifecycleEventFactory.Resumed,
    // which reads the failure context from this aggregate's public surface
    // and validates the Failed-only precondition. State change still happens
    // here, but only via Apply(WorkflowExecutionResumedEvent) on replay —
    // there is no in-aggregate command path that mutates state directly for
    // resume. This eliminates the engine-from-aggregate-mutation pattern
    // flagged in the phase-1 audit sweep (S1.2).

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case WorkflowExecutionStartedEvent e:
                Id = new WorkflowExecutionId(e.AggregateId.Value);
                _workflowName = e.WorkflowName;
                _currentStepIndex = 0;
                _completedSteps.Clear();
                _failedStepName = null;
                _failureReason = null;
                _status = WorkflowExecutionStatus.Running;
                break;
            case WorkflowStepCompletedEvent e:
                _currentStepIndex = e.StepIndex;
                _executionHash = e.ExecutionHash;
                _completedSteps.Add(e.StepName);
                break;
            case WorkflowExecutionCompletedEvent e:
                _executionHash = e.ExecutionHash;
                _status = WorkflowExecutionStatus.Completed;
                break;
            case WorkflowExecutionFailedEvent e:
                _failedStepName = e.FailedStepName;
                _failureReason = e.Reason;
                _status = WorkflowExecutionStatus.Failed;
                break;
            case WorkflowExecutionResumedEvent:
                _status = WorkflowExecutionStatus.Running;
                break;
            case WorkflowExecutionCancelledEvent:
                // R3.A.4 / R-WORKFLOW-CANCELLATION-EVENT-01: terminal
                // state. Status-only mutation — the cancel reason is
                // observability/audit data, not a replay-bearing value.
                _status = WorkflowExecutionStatus.Cancelled;
                break;
            case WorkflowExecutionSuspendedEvent:
                // R3.A.3 / R-WORKFLOW-SUSPEND-EVENT-01: NON-terminal
                // state. Status-only mutation. Resume is legal via
                // WorkflowLifecycleEventFactory.Resumed (which accepts
                // BOTH Failed and Suspended post-R3.A.3).
                _status = WorkflowExecutionStatus.Suspended;
                break;
        }
    }

    protected override void EnsureInvariants() { }
}
