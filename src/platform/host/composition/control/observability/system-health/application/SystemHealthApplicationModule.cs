using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Control.Observability.SystemHealth;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Observability.SystemHealth.Application;

public static class SystemHealthApplicationModule
{
    public static IServiceCollection AddSystemHealthApplication(this IServiceCollection services)
    {
        services.AddTransient<EvaluateSystemHealthHandler>();
        services.AddTransient<RecordSystemHealthDegradationHandler>();
        services.AddTransient<RestoreSystemHealthHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<EvaluateSystemHealthCommand, EvaluateSystemHealthHandler>();
        engine.Register<RecordSystemHealthDegradationCommand, RecordSystemHealthDegradationHandler>();
        engine.Register<RestoreSystemHealthCommand, RestoreSystemHealthHandler>();
    }
}
