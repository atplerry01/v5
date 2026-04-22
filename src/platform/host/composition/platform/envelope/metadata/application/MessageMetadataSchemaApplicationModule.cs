using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Envelope.Metadata;
using Whycespace.Shared.Contracts.Platform.Envelope.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Envelope.Metadata.Application;

public static class MessageMetadataSchemaApplicationModule
{
    public static IServiceCollection AddMessageMetadataSchemaApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterMessageMetadataSchemaHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterMessageMetadataSchemaCommand, RegisterMessageMetadataSchemaHandler>();
    }
}
