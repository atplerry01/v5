using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationDefinition.Application;

public static class ConfigurationDefinitionApplicationModule
{
    public static IServiceCollection AddConfigurationDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineConfigurationHandler>();
        services.AddTransient<DeprecateConfigurationDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineConfigurationCommand, DefineConfigurationHandler>();
        engine.Register<DeprecateConfigurationDefinitionCommand, DeprecateConfigurationDefinitionHandler>();
    }
}
