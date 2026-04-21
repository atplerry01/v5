using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.File;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.CoreObject.File.Application;

public static class DocumentFileApplicationModule
{
    public static IServiceCollection AddDocumentFileApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterDocumentFileHandler>();
        services.AddTransient<VerifyDocumentFileIntegrityHandler>();
        services.AddTransient<SupersedeDocumentFileHandler>();
        services.AddTransient<ArchiveDocumentFileHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterDocumentFileCommand, RegisterDocumentFileHandler>();
        engine.Register<VerifyDocumentFileIntegrityCommand, VerifyDocumentFileIntegrityHandler>();
        engine.Register<SupersedeDocumentFileCommand, SupersedeDocumentFileHandler>();
        engine.Register<ArchiveDocumentFileCommand, ArchiveDocumentFileHandler>();
    }
}
