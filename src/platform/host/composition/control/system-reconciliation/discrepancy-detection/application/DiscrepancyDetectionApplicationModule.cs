using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemReconciliation.DiscrepancyDetection.Application;

public static class DiscrepancyDetectionApplicationModule
{
    public static IServiceCollection AddDiscrepancyDetectionApplication(this IServiceCollection services)
    {
        services.AddTransient<DetectDiscrepancyHandler>();
        services.AddTransient<DismissDiscrepancyHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DetectDiscrepancyCommand, DetectDiscrepancyHandler>();
        engine.Register<DismissDiscrepancyCommand, DismissDiscrepancyHandler>();
    }
}
