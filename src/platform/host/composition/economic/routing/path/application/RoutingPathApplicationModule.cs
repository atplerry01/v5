using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Routing.Path.Application;

public static class RoutingPathApplicationModule
{
    public static IServiceCollection AddRoutingPathApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineRoutingPathHandler>();
        services.AddTransient<ActivateRoutingPathHandler>();
        services.AddTransient<DisableRoutingPathHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineRoutingPathCommand,   DefineRoutingPathHandler>();
        engine.Register<ActivateRoutingPathCommand, ActivateRoutingPathHandler>();
        engine.Register<DisableRoutingPathCommand,  DisableRoutingPathHandler>();
    }
}
