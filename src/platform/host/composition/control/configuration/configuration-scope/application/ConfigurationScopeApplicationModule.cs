using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationScope.Application;

public static class ConfigurationScopeApplicationModule
{
    public static IServiceCollection AddConfigurationScopeApplication(this IServiceCollection services)
    {
        services.AddTransient<DeclareConfigurationScopeHandler>();
        services.AddTransient<RemoveConfigurationScopeHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DeclareConfigurationScopeCommand, DeclareConfigurationScopeHandler>();
        engine.Register<RemoveConfigurationScopeCommand, RemoveConfigurationScopeHandler>();
    }
}
