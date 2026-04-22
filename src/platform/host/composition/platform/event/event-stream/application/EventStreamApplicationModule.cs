using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Event.EventStream;
using Whycespace.Shared.Contracts.Platform.Event.EventStream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Event.EventStream.Application;

public static class EventStreamApplicationModule
{
    public static IServiceCollection AddEventStreamApplication(this IServiceCollection services)
    {
        services.AddTransient<DeclareEventStreamHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DeclareEventStreamCommand, DeclareEventStreamHandler>();
    }
}
