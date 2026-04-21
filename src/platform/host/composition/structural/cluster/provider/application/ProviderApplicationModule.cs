using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Provider;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Provider.Application;

public static class ProviderApplicationModule
{
    public static IServiceCollection AddProviderApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterProviderHandler>();
        services.AddTransient<RegisterProviderWithParentHandler>();
        services.AddTransient<ActivateProviderHandler>();
        services.AddTransient<SuspendProviderHandler>();
        services.AddTransient<ReactivateProviderHandler>();
        services.AddTransient<RetireProviderHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterProviderCommand, RegisterProviderHandler>();
        engine.Register<RegisterProviderWithParentCommand, RegisterProviderWithParentHandler>();
        engine.Register<ActivateProviderCommand, ActivateProviderHandler>();
        engine.Register<SuspendProviderCommand, SuspendProviderHandler>();
        engine.Register<ReactivateProviderCommand, ReactivateProviderHandler>();
        engine.Register<RetireProviderCommand, RetireProviderHandler>();
    }
}
