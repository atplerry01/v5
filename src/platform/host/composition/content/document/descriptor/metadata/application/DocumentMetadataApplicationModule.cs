using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.Descriptor.Metadata.Application;

public static class DocumentMetadataApplicationModule
{
    public static IServiceCollection AddDocumentMetadataApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateDocumentMetadataHandler>();
        services.AddTransient<AddDocumentMetadataEntryHandler>();
        services.AddTransient<UpdateDocumentMetadataEntryHandler>();
        services.AddTransient<RemoveDocumentMetadataEntryHandler>();
        services.AddTransient<FinalizeDocumentMetadataHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDocumentMetadataCommand, CreateDocumentMetadataHandler>();
        engine.Register<AddDocumentMetadataEntryCommand, AddDocumentMetadataEntryHandler>();
        engine.Register<UpdateDocumentMetadataEntryCommand, UpdateDocumentMetadataEntryHandler>();
        engine.Register<RemoveDocumentMetadataEntryCommand, RemoveDocumentMetadataEntryHandler>();
        engine.Register<FinalizeDocumentMetadataCommand, FinalizeDocumentMetadataHandler>();
    }
}
