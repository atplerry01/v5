using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.Steps;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Revenue.Workflow;

/// <summary>
/// Revenue processing workflow module — T1M step DI registrations and
/// workflow registry binding for "economic.revenue.process".
/// </summary>
public static class RevenueProcessingWorkflowModule
{
    public static IServiceCollection AddRevenueProcessingWorkflow(this IServiceCollection services)
    {
        services.AddTransient<ValidateRevenueStep>();
        services.AddTransient<ApplyRevenueStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(RevenueProcessingWorkflowNames.Process, new[]
        {
            typeof(ValidateRevenueStep),
            typeof(ApplyRevenueStep)
        });
    }
}
