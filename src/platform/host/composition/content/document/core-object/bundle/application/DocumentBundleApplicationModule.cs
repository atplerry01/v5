using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Bundle.Application;

public static class DocumentBundleApplicationModule
{
    public static IServiceCollection AddDocumentBundleApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateDocumentBundleHandler>();
        services.AddTransient<RenameDocumentBundleHandler>();
        services.AddTransient<AddDocumentBundleMemberHandler>();
        services.AddTransient<RemoveDocumentBundleMemberHandler>();
        services.AddTransient<FinalizeDocumentBundleHandler>();
        services.AddTransient<ArchiveDocumentBundleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDocumentBundleCommand, CreateDocumentBundleHandler>();
        engine.Register<RenameDocumentBundleCommand, RenameDocumentBundleHandler>();
        engine.Register<AddDocumentBundleMemberCommand, AddDocumentBundleMemberHandler>();
        engine.Register<RemoveDocumentBundleMemberCommand, RemoveDocumentBundleMemberHandler>();
        engine.Register<FinalizeDocumentBundleCommand, FinalizeDocumentBundleHandler>();
        engine.Register<ArchiveDocumentBundleCommand, ArchiveDocumentBundleHandler>();
    }
}
