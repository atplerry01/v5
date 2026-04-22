using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemReconciliation.DiscrepancyResolution.Application;

public static class DiscrepancyResolutionApplicationModule
{
    public static IServiceCollection AddDiscrepancyResolutionApplication(this IServiceCollection services)
    {
        services.AddTransient<InitiateDiscrepancyResolutionHandler>();
        services.AddTransient<CompleteDiscrepancyResolutionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<InitiateDiscrepancyResolutionCommand, InitiateDiscrepancyResolutionHandler>();
        engine.Register<CompleteDiscrepancyResolutionCommand, CompleteDiscrepancyResolutionHandler>();
    }
}
