using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Template.Application;

public static class DocumentTemplateApplicationModule
{
    public static IServiceCollection AddDocumentTemplateApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateDocumentTemplateHandler>();
        services.AddTransient<UpdateDocumentTemplateHandler>();
        services.AddTransient<ActivateDocumentTemplateHandler>();
        services.AddTransient<DeprecateDocumentTemplateHandler>();
        services.AddTransient<ArchiveDocumentTemplateHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDocumentTemplateCommand, CreateDocumentTemplateHandler>();
        engine.Register<UpdateDocumentTemplateCommand, UpdateDocumentTemplateHandler>();
        engine.Register<ActivateDocumentTemplateCommand, ActivateDocumentTemplateHandler>();
        engine.Register<DeprecateDocumentTemplateCommand, DeprecateDocumentTemplateHandler>();
        engine.Register<ArchiveDocumentTemplateCommand, ArchiveDocumentTemplateHandler>();
    }
}
