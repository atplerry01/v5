using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.StreamCore.Manifest.Application;

public static class ManifestApplicationModule
{
    public static IServiceCollection AddManifestApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateManifestHandler>();
        services.AddTransient<UpdateManifestHandler>();
        services.AddTransient<PublishManifestHandler>();
        services.AddTransient<RetireManifestHandler>();
        services.AddTransient<ArchiveManifestHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateManifestCommand, CreateManifestHandler>();
        engine.Register<UpdateManifestCommand, UpdateManifestHandler>();
        engine.Register<PublishManifestCommand, PublishManifestHandler>();
        engine.Register<RetireManifestCommand, RetireManifestHandler>();
        engine.Register<ArchiveManifestCommand, ArchiveManifestHandler>();
    }
}
