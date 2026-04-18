using Whycespace.Engines.T1M.Domains.Economic.Enforcement.State;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Economic.Enforcement.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Enforcement.Steps;

/// <summary>
/// Step 4: Lock the system for the subject — hard stop that prevents ALL
/// command execution for the subject. Only applies for Critical severity.
/// For non-Critical severity, this step succeeds as a no-op.
/// </summary>
public sealed class LockSystemStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IEconomicMetrics _metrics;

    public LockSystemStep(
        ISystemIntentDispatcher dispatcher,
        IEconomicMetrics metrics)
    {
        _dispatcher = dispatcher;
        _metrics = metrics;
    }

    public string Name => EnforcementLifecycleSteps.LockSystem;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute LockRoute = new("economic", "enforcement", "lock");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<EnforcementLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("EnforcementLifecycleWorkflowState not found in workflow context.");

        // Lock only applies for Critical severity.
        if (!string.Equals(state.Severity, "Critical", StringComparison.Ordinal))
        {
            state.CurrentStep = EnforcementLifecycleSteps.LockSystem;
            context.SetState(state);
            return WorkflowStepResult.Success(state.LockId);
        }

        var command = new LockSystemCommand(
            state.LockId,
            state.SubjectId,
            state.Severity,
            state.Reason,
            state.DetectedAt);

        var result = await _dispatcher.DispatchSystemAsync(command, LockRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "LockSystem dispatch failed.");

        _metrics.RecordLockApplied(state.Severity);

        state.CurrentStep = EnforcementLifecycleSteps.LockSystem;
        context.SetState(state);

        return WorkflowStepResult.Success(state.LockId, result.EmittedEvents);
    }
}
