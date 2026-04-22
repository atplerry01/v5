using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Control.Observability.SystemMetric;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Observability.SystemMetric.Application;

public static class SystemMetricApplicationModule
{
    public static IServiceCollection AddSystemMetricApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineSystemMetricHandler>();
        services.AddTransient<DeprecateSystemMetricHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineSystemMetricCommand, DefineSystemMetricHandler>();
        engine.Register<DeprecateSystemMetricCommand, DeprecateSystemMetricHandler>();
    }
}
