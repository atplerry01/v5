using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.PlaybackConsumption.Progress.Application;

public static class ProgressApplicationModule
{
    public static IServiceCollection AddProgressApplication(this IServiceCollection services)
    {
        services.AddTransient<TrackProgressHandler>();
        services.AddTransient<UpdatePlaybackPositionHandler>();
        services.AddTransient<PausePlaybackHandler>();
        services.AddTransient<ResumePlaybackHandler>();
        services.AddTransient<TerminateProgressHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<TrackProgressCommand, TrackProgressHandler>();
        engine.Register<UpdatePlaybackPositionCommand, UpdatePlaybackPositionHandler>();
        engine.Register<PausePlaybackCommand, PausePlaybackHandler>();
        engine.Register<ResumePlaybackCommand, ResumePlaybackHandler>();
        engine.Register<TerminateProgressCommand, TerminateProgressHandler>();
    }
}
