using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.LiveStreaming.Archive.Application;

public static class ArchiveApplicationModule
{
    public static IServiceCollection AddArchiveApplication(this IServiceCollection services)
    {
        services.AddTransient<StartArchiveHandler>();
        services.AddTransient<CompleteArchiveHandler>();
        services.AddTransient<FailArchiveHandler>();
        services.AddTransient<FinalizeArchiveHandler>();
        services.AddTransient<ArchiveArchiveHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<StartArchiveCommand, StartArchiveHandler>();
        engine.Register<CompleteArchiveCommand, CompleteArchiveHandler>();
        engine.Register<FailArchiveCommand, FailArchiveHandler>();
        engine.Register<FinalizeArchiveCommand, FinalizeArchiveHandler>();
        engine.Register<ArchiveArchiveCommand, ArchiveArchiveHandler>();
    }
}
