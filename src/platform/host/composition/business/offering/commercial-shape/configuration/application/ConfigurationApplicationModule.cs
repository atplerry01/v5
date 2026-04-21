using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Offering.CommercialShape.Configuration.Application;

public static class ConfigurationApplicationModule
{
    public static IServiceCollection AddConfigurationApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateConfigurationHandler>();
        services.AddTransient<SetConfigurationOptionHandler>();
        services.AddTransient<RemoveConfigurationOptionHandler>();
        services.AddTransient<ActivateConfigurationHandler>();
        services.AddTransient<ArchiveConfigurationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateConfigurationCommand, CreateConfigurationHandler>();
        engine.Register<SetConfigurationOptionCommand, SetConfigurationOptionHandler>();
        engine.Register<RemoveConfigurationOptionCommand, RemoveConfigurationOptionHandler>();
        engine.Register<ActivateConfigurationCommand, ActivateConfigurationHandler>();
        engine.Register<ArchiveConfigurationCommand, ArchiveConfigurationHandler>();
    }
}
