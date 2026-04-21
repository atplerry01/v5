using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Document.Application;

public static class DocumentApplicationModule
{
    public static IServiceCollection AddDocumentApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateDocumentHandler>();
        services.AddTransient<UpdateDocumentMetadataHandler>();
        services.AddTransient<AttachDocumentVersionHandler>();
        services.AddTransient<ActivateDocumentHandler>();
        services.AddTransient<ArchiveDocumentHandler>();
        services.AddTransient<RestoreDocumentHandler>();
        services.AddTransient<SupersedeDocumentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDocumentCommand, CreateDocumentHandler>();
        engine.Register<UpdateDocumentMetadataCommand, UpdateDocumentMetadataHandler>();
        engine.Register<AttachDocumentVersionCommand, AttachDocumentVersionHandler>();
        engine.Register<ActivateDocumentCommand, ActivateDocumentHandler>();
        engine.Register<ArchiveDocumentCommand, ArchiveDocumentHandler>();
        engine.Register<RestoreDocumentCommand, RestoreDocumentHandler>();
        engine.Register<SupersedeDocumentCommand, SupersedeDocumentHandler>();
    }
}
