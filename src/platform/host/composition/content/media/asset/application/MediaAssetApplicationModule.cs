using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.Asset;
using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.Asset.Application;

public static class MediaAssetApplicationModule
{
    public static IServiceCollection AddMediaAssetApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterMediaAssetHandler>();
        services.AddTransient<StartMediaAssetProcessingHandler>();
        services.AddTransient<MarkMediaAssetAvailableHandler>();
        services.AddTransient<PublishMediaAssetHandler>();
        services.AddTransient<UnpublishMediaAssetHandler>();
        services.AddTransient<ArchiveMediaAssetHandler>();
        services.AddTransient<UpdateMediaAssetMetadataHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterMediaAssetCommand, RegisterMediaAssetHandler>();
        engine.Register<StartMediaAssetProcessingCommand, StartMediaAssetProcessingHandler>();
        engine.Register<MarkMediaAssetAvailableCommand, MarkMediaAssetAvailableHandler>();
        engine.Register<PublishMediaAssetCommand, PublishMediaAssetHandler>();
        engine.Register<UnpublishMediaAssetCommand, UnpublishMediaAssetHandler>();
        engine.Register<ArchiveMediaAssetCommand, ArchiveMediaAssetHandler>();
        engine.Register<UpdateMediaAssetMetadataCommand, UpdateMediaAssetMetadataHandler>();
    }
}
