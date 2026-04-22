using Whycespace.Engines.T1M.Domains.Trust.Identity.Registry.State;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Registry.Workflow;

namespace Whycespace.Engines.T1M.Domains.Trust.Identity.Registry.Steps;

public sealed class ValidateRegistrationStep : IWorkflowStep
{
    public string Name => RegistrationOnboardingSteps.Validate;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not RegistrationOnboardingIntent intent)
            return Task.FromResult(
                WorkflowStepResult.Failure("Payload is not a valid RegistrationOnboardingIntent."));

        if (intent.RegistryId == Guid.Empty)
            return Task.FromResult(WorkflowStepResult.Failure("RegistryId is required."));
        if (string.IsNullOrWhiteSpace(intent.Email))
            return Task.FromResult(WorkflowStepResult.Failure("Email is required."));
        if (string.IsNullOrWhiteSpace(intent.RegistrationType))
            return Task.FromResult(WorkflowStepResult.Failure("RegistrationType is required."));

        var state = new RegistrationOnboardingWorkflowState
        {
            RegistryId       = intent.RegistryId,
            Email            = intent.Email,
            RegistrationType = intent.RegistrationType,
            CurrentStep      = RegistrationOnboardingSteps.Validate
        };
        context.SetState(state);

        return Task.FromResult(WorkflowStepResult.Success(intent));
    }
}
