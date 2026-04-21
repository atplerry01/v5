using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.LiveStreaming.Broadcast.Application;

public static class BroadcastApplicationModule
{
    public static IServiceCollection AddBroadcastApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateBroadcastHandler>();
        services.AddTransient<ScheduleBroadcastHandler>();
        services.AddTransient<StartBroadcastHandler>();
        services.AddTransient<PauseBroadcastHandler>();
        services.AddTransient<ResumeBroadcastHandler>();
        services.AddTransient<EndBroadcastHandler>();
        services.AddTransient<CancelBroadcastHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateBroadcastCommand, CreateBroadcastHandler>();
        engine.Register<ScheduleBroadcastCommand, ScheduleBroadcastHandler>();
        engine.Register<StartBroadcastCommand, StartBroadcastHandler>();
        engine.Register<PauseBroadcastCommand, PauseBroadcastHandler>();
        engine.Register<ResumeBroadcastCommand, ResumeBroadcastHandler>();
        engine.Register<EndBroadcastCommand, EndBroadcastHandler>();
        engine.Register<CancelBroadcastCommand, CancelBroadcastHandler>();
    }
}
