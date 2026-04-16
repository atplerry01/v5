using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Reconciliation.Discrepancy.Application;

public static class ReconciliationDiscrepancyApplicationModule
{
    public static IServiceCollection AddReconciliationDiscrepancyApplication(this IServiceCollection services)
    {
        services.AddTransient<DetectDiscrepancyHandler>();
        services.AddTransient<InvestigateDiscrepancyHandler>();
        services.AddTransient<ResolveDiscrepancyHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DetectDiscrepancyCommand,      DetectDiscrepancyHandler>();
        engine.Register<InvestigateDiscrepancyCommand, InvestigateDiscrepancyHandler>();
        engine.Register<ResolveDiscrepancyCommand,     ResolveDiscrepancyHandler>();
    }
}
