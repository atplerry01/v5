using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.LifecycleChange.Version.Application;

public static class DocumentVersionApplicationModule
{
    public static IServiceCollection AddDocumentVersionApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateDocumentVersionHandler>();
        services.AddTransient<ActivateDocumentVersionHandler>();
        services.AddTransient<SupersedeDocumentVersionHandler>();
        services.AddTransient<WithdrawDocumentVersionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDocumentVersionCommand, CreateDocumentVersionHandler>();
        engine.Register<ActivateDocumentVersionCommand, ActivateDocumentVersionHandler>();
        engine.Register<SupersedeDocumentVersionCommand, SupersedeDocumentVersionHandler>();
        engine.Register<WithdrawDocumentVersionCommand, WithdrawDocumentVersionHandler>();
    }
}
