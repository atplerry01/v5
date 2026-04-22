using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Trust.Identity.Registry.Steps;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Registry.Workflow;

namespace Whycespace.Platform.Host.Composition.Trust.Identity.Registry.Workflow;

public static class RegistrationOnboardingWorkflowModule
{
    public static IServiceCollection AddRegistrationOnboardingWorkflow(this IServiceCollection services)
    {
        services.AddTransient<ValidateRegistrationStep>();
        services.AddTransient<VerifyRegistrationStep>();
        services.AddTransient<ActivateRegistrationStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(RegistrationOnboardingWorkflowNames.Onboard, new[]
        {
            typeof(ValidateRegistrationStep),
            typeof(VerifyRegistrationStep),
            typeof(ActivateRegistrationStep)
        });
    }
}
