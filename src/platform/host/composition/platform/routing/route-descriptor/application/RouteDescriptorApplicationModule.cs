using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Routing.RouteDescriptor;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDescriptor;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Routing.RouteDescriptor.Application;

public static class RouteDescriptorApplicationModule
{
    public static IServiceCollection AddRouteDescriptorApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterRouteDescriptorHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterRouteDescriptorCommand, RegisterRouteDescriptorHandler>();
    }
}
