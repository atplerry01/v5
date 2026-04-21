using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.CoreObject.Subtitle.Application;

public static class SubtitleApplicationModule
{
    public static IServiceCollection AddSubtitleApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateSubtitleHandler>();
        services.AddTransient<UpdateSubtitleHandler>();
        services.AddTransient<FinalizeSubtitleHandler>();
        services.AddTransient<ArchiveSubtitleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateSubtitleCommand, CreateSubtitleHandler>();
        engine.Register<UpdateSubtitleCommand, UpdateSubtitleHandler>();
        engine.Register<FinalizeSubtitleCommand, FinalizeSubtitleHandler>();
        engine.Register<ArchiveSubtitleCommand, ArchiveSubtitleHandler>();
    }
}
