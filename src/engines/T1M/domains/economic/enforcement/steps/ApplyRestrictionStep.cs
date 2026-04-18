using Whycespace.Engines.T1M.Domains.Economic.Enforcement.State;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Enforcement.Steps;

/// <summary>
/// Step 3: Apply a restriction — restricts the subject's ability to execute
/// commands in the enforced scope. Only applies for High and Critical severity.
/// For Low/Medium severity, this step succeeds as a no-op.
/// </summary>
public sealed class ApplyRestrictionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public ApplyRestrictionStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => EnforcementLifecycleSteps.ApplyRestriction;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute RestrictionRoute = new("economic", "enforcement", "restriction");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<EnforcementLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("EnforcementLifecycleWorkflowState not found in workflow context.");

        // Restriction only applies for High and Critical severity.
        if (!string.Equals(state.Severity, "High", StringComparison.Ordinal) &&
            !string.Equals(state.Severity, "Critical", StringComparison.Ordinal))
        {
            state.CurrentStep = EnforcementLifecycleSteps.ApplyRestriction;
            context.SetState(state);
            return WorkflowStepResult.Success(state.RestrictionId);
        }

        var command = new ApplyRestrictionCommand(
            state.RestrictionId,
            state.SubjectId,
            state.Severity,
            state.Reason,
            state.DetectedAt);

        var result = await _dispatcher.DispatchSystemAsync(command, RestrictionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "ApplyRestriction dispatch failed.");

        state.CurrentStep = EnforcementLifecycleSteps.ApplyRestriction;
        context.SetState(state);

        return WorkflowStepResult.Success(state.RestrictionId, result.EmittedEvents);
    }
}
