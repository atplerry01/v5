using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Configuration.ConfigurationAssignment.Application;

public static class ConfigurationAssignmentApplicationModule
{
    public static IServiceCollection AddConfigurationAssignmentApplication(this IServiceCollection services)
    {
        services.AddTransient<AssignConfigurationHandler>();
        services.AddTransient<RevokeConfigurationAssignmentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<AssignConfigurationCommand, AssignConfigurationHandler>();
        engine.Register<RevokeConfigurationAssignmentCommand, RevokeConfigurationAssignmentHandler>();
    }
}
