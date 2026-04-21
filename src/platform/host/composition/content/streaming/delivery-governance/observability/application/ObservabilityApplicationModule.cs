using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.DeliveryGovernance.Observability.Application;

public static class ObservabilityApplicationModule
{
    public static IServiceCollection AddObservabilityApplication(this IServiceCollection services)
    {
        services.AddTransient<CaptureObservabilityHandler>();
        services.AddTransient<UpdateObservabilityHandler>();
        services.AddTransient<FinalizeObservabilityHandler>();
        services.AddTransient<ArchiveObservabilityHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CaptureObservabilityCommand, CaptureObservabilityHandler>();
        engine.Register<UpdateObservabilityCommand, UpdateObservabilityHandler>();
        engine.Register<FinalizeObservabilityCommand, FinalizeObservabilityHandler>();
        engine.Register<ArchiveObservabilityCommand, ArchiveObservabilityHandler>();
    }
}
