using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.CoreObject.Transcript.Application;

public static class TranscriptApplicationModule
{
    public static IServiceCollection AddTranscriptApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateTranscriptHandler>();
        services.AddTransient<UpdateTranscriptHandler>();
        services.AddTransient<FinalizeTranscriptHandler>();
        services.AddTransient<ArchiveTranscriptHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateTranscriptCommand, CreateTranscriptHandler>();
        engine.Register<UpdateTranscriptCommand, UpdateTranscriptHandler>();
        engine.Register<FinalizeTranscriptCommand, FinalizeTranscriptHandler>();
        engine.Register<ArchiveTranscriptCommand, ArchiveTranscriptHandler>();
    }
}
