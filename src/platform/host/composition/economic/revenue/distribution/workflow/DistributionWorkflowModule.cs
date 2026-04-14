using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;
using Whycespace.Engines.T2E.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Distribution.Workflow;

/// <summary>
/// Distribution workflow module — T1M step + T2E handler DI registrations
/// and workflow registry binding for "economic.distribution.create".
/// </summary>
public static class DistributionWorkflowModule
{
    public static IServiceCollection AddDistributionWorkflow(this IServiceCollection services)
    {
        services.AddTransient<ValidateDistributionStep>();
        services.AddTransient<CreateDistributionStep>();
        services.AddTransient<CreateDistributionHandler>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(DistributionWorkflowNames.Create, new[]
        {
            typeof(ValidateDistributionStep),
            typeof(CreateDistributionStep)
        });
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDistributionCommand, CreateDistributionHandler>();
    }
}
