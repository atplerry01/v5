using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationState.Application;

public static class ConfigurationStateApplicationModule
{
    public static IServiceCollection AddConfigurationStateApplication(this IServiceCollection services)
    {
        services.AddTransient<SetConfigurationStateHandler>();
        services.AddTransient<RevokeConfigurationStateHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<SetConfigurationStateCommand, SetConfigurationStateHandler>();
        engine.Register<RevokeConfigurationStateCommand, RevokeConfigurationStateHandler>();
    }
}
