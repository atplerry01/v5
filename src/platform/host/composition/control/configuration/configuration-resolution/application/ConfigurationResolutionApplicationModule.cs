using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationResolution.Application;

public static class ConfigurationResolutionApplicationModule
{
    public static IServiceCollection AddConfigurationResolutionApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordConfigurationResolutionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordConfigurationResolutionCommand, RecordConfigurationResolutionHandler>();
    }
}
