using Whycespace.Engines.T1M.Domains.Trust.Identity.Registry.State;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;

namespace Whycespace.Engines.T1M.Domains.Trust.Identity.Registry.Steps;

public sealed class ActivateRegistrationStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public ActivateRegistrationStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => RegistrationOnboardingSteps.Activate;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute RegistryRoute = new("trust", "identity", "registry");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<RegistrationOnboardingWorkflowState>()
            ?? throw new InvalidOperationException("RegistrationOnboardingWorkflowState not found in workflow context.");

        var command = new ActivateRegistrationCommand(state.RegistryId);

        var result = await _dispatcher.DispatchSystemAsync(command, RegistryRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "ActivateRegistration dispatch failed.");

        state.CurrentStep = RegistrationOnboardingSteps.Activate;
        context.SetState(state);

        return WorkflowStepResult.Success(state.RegistryId, result.EmittedEvents);
    }
}
