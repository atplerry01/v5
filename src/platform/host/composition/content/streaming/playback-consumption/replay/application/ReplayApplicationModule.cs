using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.PlaybackConsumption.Replay.Application;

public static class ReplayApplicationModule
{
    public static IServiceCollection AddReplayApplication(this IServiceCollection services)
    {
        services.AddTransient<RequestReplayHandler>();
        services.AddTransient<StartReplayHandler>();
        services.AddTransient<PauseReplayHandler>();
        services.AddTransient<ResumeReplayHandler>();
        services.AddTransient<CompleteReplayHandler>();
        services.AddTransient<AbandonReplayHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RequestReplayCommand, RequestReplayHandler>();
        engine.Register<StartReplayCommand, StartReplayHandler>();
        engine.Register<PauseReplayCommand, PauseReplayHandler>();
        engine.Register<ResumeReplayCommand, ResumeReplayHandler>();
        engine.Register<CompleteReplayCommand, CompleteReplayHandler>();
        engine.Register<AbandonReplayCommand, AbandonReplayHandler>();
    }
}
