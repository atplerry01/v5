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
///
/// Phase 3 (LOCKED): chain is
///   EnsureContractActive → Validate → Create → Confirm → TriggerPayout.
/// Only Confirmed distributions trigger payout. ConfirmDistribution and
/// TriggerPayout are dispatched via DispatchSystemAsync — no manual API.
/// </summary>
public static class DistributionWorkflowModule
{
    public static IServiceCollection AddDistributionWorkflow(this IServiceCollection services)
    {
        services.AddTransient<EnsureContractActiveStep>();
        services.AddTransient<ValidateDistributionStep>();
        services.AddTransient<CreateDistributionStep>();
        services.AddTransient<ConfirmDistributionStep>();
        services.AddTransient<TriggerPayoutStep>();

        services.AddTransient<CreateDistributionHandler>();
        services.AddTransient<ConfirmDistributionHandler>();
        services.AddTransient<MarkDistributionPaidHandler>();
        services.AddTransient<MarkDistributionFailedHandler>();
        services.AddTransient<RequestDistributionCompensationHandler>();
        services.AddTransient<MarkDistributionCompensatedHandler>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(DistributionWorkflowNames.Create, new[]
        {
            typeof(EnsureContractActiveStep),
            typeof(ValidateDistributionStep),
            typeof(CreateDistributionStep),
            typeof(ConfirmDistributionStep),
            typeof(TriggerPayoutStep)
        });
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDistributionCommand, CreateDistributionHandler>();
        engine.Register<ConfirmDistributionCommand, ConfirmDistributionHandler>();
        engine.Register<MarkDistributionPaidCommand, MarkDistributionPaidHandler>();
        engine.Register<MarkDistributionFailedCommand, MarkDistributionFailedHandler>();
        engine.Register<RequestDistributionCompensationCommand, RequestDistributionCompensationHandler>();
        engine.Register<MarkDistributionCompensatedCommand, MarkDistributionCompensatedHandler>();
    }
}
