using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.Descriptor.Metadata.Application;

public static class MediaMetadataApplicationModule
{
    public static IServiceCollection AddMediaMetadataApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateMediaMetadataHandler>();
        services.AddTransient<AddMediaMetadataEntryHandler>();
        services.AddTransient<UpdateMediaMetadataEntryHandler>();
        services.AddTransient<RemoveMediaMetadataEntryHandler>();
        services.AddTransient<FinalizeMediaMetadataHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateMediaMetadataCommand, CreateMediaMetadataHandler>();
        engine.Register<AddMediaMetadataEntryCommand, AddMediaMetadataEntryHandler>();
        engine.Register<UpdateMediaMetadataEntryCommand, UpdateMediaMetadataEntryHandler>();
        engine.Register<RemoveMediaMetadataEntryCommand, RemoveMediaMetadataEntryHandler>();
        engine.Register<FinalizeMediaMetadataCommand, FinalizeMediaMetadataHandler>();
    }
}
