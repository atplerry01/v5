using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Event.EventMetadata;
using Whycespace.Shared.Contracts.Platform.Event.EventMetadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Event.EventMetadata.Application;

public static class EventMetadataApplicationModule
{
    public static IServiceCollection AddEventMetadataApplication(this IServiceCollection services)
    {
        services.AddTransient<AttachEventMetadataHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<AttachEventMetadataCommand, AttachEventMetadataHandler>();
    }
}
