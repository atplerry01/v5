using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Routing.RouteDefinition;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Routing.RouteDefinition.Application;

public static class RouteDefinitionApplicationModule
{
    public static IServiceCollection AddRouteDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterRouteDefinitionHandler>();
        services.AddTransient<DeactivateRouteDefinitionHandler>();
        services.AddTransient<DeprecateRouteDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterRouteDefinitionCommand, RegisterRouteDefinitionHandler>();
        engine.Register<DeactivateRouteDefinitionCommand, DeactivateRouteDefinitionHandler>();
        engine.Register<DeprecateRouteDefinitionCommand, DeprecateRouteDefinitionHandler>();
    }
}
