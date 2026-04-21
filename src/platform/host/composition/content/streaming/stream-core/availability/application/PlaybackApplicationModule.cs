using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.StreamCore.Availability.Application;

public static class PlaybackApplicationModule
{
    public static IServiceCollection AddPlaybackApplication(this IServiceCollection services)
    {
        services.AddTransient<CreatePlaybackHandler>();
        services.AddTransient<EnablePlaybackHandler>();
        services.AddTransient<DisablePlaybackHandler>();
        services.AddTransient<UpdatePlaybackWindowHandler>();
        services.AddTransient<ArchivePlaybackHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreatePlaybackCommand, CreatePlaybackHandler>();
        engine.Register<EnablePlaybackCommand, EnablePlaybackHandler>();
        engine.Register<DisablePlaybackCommand, DisablePlaybackHandler>();
        engine.Register<UpdatePlaybackWindowCommand, UpdatePlaybackWindowHandler>();
        engine.Register<ArchivePlaybackCommand, ArchivePlaybackHandler>();
    }
}
