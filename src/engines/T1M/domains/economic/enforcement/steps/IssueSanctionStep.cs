using Whycespace.Engines.T1M.Domains.Economic.Enforcement.State;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Enforcement.Steps;

/// <summary>
/// Step 2: Issue a sanction — creates the formal enforcement sanction
/// based on the violation severity and recommended action.
/// </summary>
public sealed class IssueSanctionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IEconomicMetrics _metrics;

    public IssueSanctionStep(
        ISystemIntentDispatcher dispatcher,
        IEconomicMetrics metrics)
    {
        _dispatcher = dispatcher;
        _metrics = metrics;
    }

    public string Name => EnforcementLifecycleSteps.IssueSanction;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute SanctionRoute = new("economic", "enforcement", "sanction");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<EnforcementLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("EnforcementLifecycleWorkflowState not found in workflow context.");

        var command = new IssueSanctionCommand(
            state.SanctionId,
            state.SubjectId,
            state.RecommendedAction,
            state.Severity,
            state.Reason,
            state.DetectedAt,
            ExpiresAt: null,
            state.DetectedAt);

        var result = await _dispatcher.DispatchSystemAsync(command, SanctionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "IssueSanction dispatch failed.");

        _metrics.RecordSanctionIssued(state.Severity);

        state.CurrentStep = EnforcementLifecycleSteps.IssueSanction;
        context.SetState(state);

        return WorkflowStepResult.Success(state.SanctionId, result.EmittedEvents);
    }
}
