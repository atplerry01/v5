using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.StreamCore.Stream.Application;

public static class StreamApplicationModule
{
    public static IServiceCollection AddStreamApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateStreamHandler>();
        services.AddTransient<ActivateStreamHandler>();
        services.AddTransient<PauseStreamHandler>();
        services.AddTransient<ResumeStreamHandler>();
        services.AddTransient<EndStreamHandler>();
        services.AddTransient<ArchiveStreamHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateStreamCommand, CreateStreamHandler>();
        engine.Register<ActivateStreamCommand, ActivateStreamHandler>();
        engine.Register<PauseStreamCommand, PauseStreamHandler>();
        engine.Register<ResumeStreamCommand, ResumeStreamHandler>();
        engine.Register<EndStreamCommand, EndStreamHandler>();
        engine.Register<ArchiveStreamCommand, ArchiveStreamHandler>();
    }
}
