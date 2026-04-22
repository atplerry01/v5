using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Event.EventSchema;
using Whycespace.Shared.Contracts.Platform.Event.EventSchema;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Event.EventSchema.Application;

public static class EventSchemaApplicationModule
{
    public static IServiceCollection AddEventSchemaApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterEventSchemaHandler>();
        services.AddTransient<DeprecateEventSchemaHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterEventSchemaCommand, RegisterEventSchemaHandler>();
        engine.Register<DeprecateEventSchemaCommand, DeprecateEventSchemaHandler>();
    }
}
