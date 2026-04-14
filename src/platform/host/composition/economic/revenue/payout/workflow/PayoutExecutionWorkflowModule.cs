using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Payout.Workflow;

/// <summary>
/// Payout execution workflow module — T1M step DI registrations and
/// workflow registry binding for "economic.payout.execute". Reuses the
/// DebitSliceHandler / CreditSliceHandler wired by VaultAccountApplicationModule.
/// </summary>
public static class PayoutExecutionWorkflowModule
{
    public static IServiceCollection AddPayoutExecutionWorkflow(this IServiceCollection services)
    {
        services.AddTransient<LoadDistributionStep>();
        services.AddTransient<ExecutePayoutStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(PayoutExecutionWorkflowNames.Execute, new[]
        {
            typeof(LoadDistributionStep),
            typeof(ExecutePayoutStep)
        });
    }
}
