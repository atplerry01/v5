using Whycespace.Engines.T1M.Domains.Economic.Enforcement.State;
using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Economic.Enforcement.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Enforcement.Steps;

/// <summary>
/// Step 1: Escalate the violation — accumulates the violation against the
/// subject's escalation aggregate so the per-subject violation count, score,
/// and escalation level are updated.
/// </summary>
public sealed class EscalateViolationStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IEconomicMetrics _metrics;

    public EscalateViolationStep(
        ISystemIntentDispatcher dispatcher,
        IEconomicMetrics metrics)
    {
        _dispatcher = dispatcher;
        _metrics = metrics;
    }

    public string Name => EnforcementLifecycleSteps.EscalateViolation;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute EscalationRoute = new("economic", "enforcement", "escalation");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<EnforcementLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("EnforcementLifecycleWorkflowState not found in workflow context.");

        _metrics.RecordEnforcementWorkflowTriggered(state.Severity);

        var command = new AccumulateViolationCommand(
            state.SubjectId,
            state.ViolationId,
            state.Severity,
            state.DetectedAt);

        var result = await _dispatcher.DispatchSystemAsync(command, EscalationRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "AccumulateViolation dispatch failed.");

        state.CurrentStep = EnforcementLifecycleSteps.EscalateViolation;
        context.SetState(state);

        return WorkflowStepResult.Success(state.SubjectId, result.EmittedEvents);
    }
}
