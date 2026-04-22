using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.LiveStreaming.IngestSession.Application;

public static class IngestSessionApplicationModule
{
    public static IServiceCollection AddIngestSessionApplication(this IServiceCollection services)
    {
        services.AddTransient<AuthenticateIngestSessionHandler>();
        services.AddTransient<StartIngestStreamingHandler>();
        services.AddTransient<StallIngestSessionHandler>();
        services.AddTransient<ResumeIngestSessionHandler>();
        services.AddTransient<EndIngestSessionHandler>();
        services.AddTransient<FailIngestSessionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<AuthenticateIngestSessionCommand, AuthenticateIngestSessionHandler>();
        engine.Register<StartIngestStreamingCommand, StartIngestStreamingHandler>();
        engine.Register<StallIngestSessionCommand, StallIngestSessionHandler>();
        engine.Register<ResumeIngestSessionCommand, ResumeIngestSessionHandler>();
        engine.Register<EndIngestSessionCommand, EndIngestSessionHandler>();
        engine.Register<FailIngestSessionCommand, FailIngestSessionHandler>();
    }
}
