using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Event.EventDefinition;
using Whycespace.Shared.Contracts.Platform.Event.EventDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Event.EventDefinition.Application;

public static class EventDefinitionApplicationModule
{
    public static IServiceCollection AddEventDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineEventHandler>();
        services.AddTransient<DeprecateEventDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineEventCommand, DefineEventHandler>();
        engine.Register<DeprecateEventDefinitionCommand, DeprecateEventDefinitionHandler>();
    }
}
