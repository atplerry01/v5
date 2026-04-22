using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Command.CommandMetadata;
using Whycespace.Shared.Contracts.Platform.Command.CommandMetadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Command.CommandMetadata.Application;

public static class CommandMetadataApplicationModule
{
    public static IServiceCollection AddCommandMetadataApplication(this IServiceCollection services)
    {
        services.AddTransient<AttachCommandMetadataHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<AttachCommandMetadataCommand, AttachCommandMetadataHandler>();
    }
}
