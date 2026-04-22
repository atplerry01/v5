using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Routing.RouteResolution;
using Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Routing.RouteResolution.Application;

public static class RouteResolutionApplicationModule
{
    public static IServiceCollection AddRouteResolutionApplication(this IServiceCollection services)
    {
        services.AddTransient<ResolveRouteHandler>();
        services.AddTransient<FailRouteResolutionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<ResolveRouteCommand, ResolveRouteHandler>();
        engine.Register<FailRouteResolutionCommand, FailRouteResolutionHandler>();
    }
}
