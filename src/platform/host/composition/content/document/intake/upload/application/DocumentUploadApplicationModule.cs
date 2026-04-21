using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.Intake.Upload.Application;

public static class DocumentUploadApplicationModule
{
    public static IServiceCollection AddDocumentUploadApplication(this IServiceCollection services)
    {
        services.AddTransient<RequestDocumentUploadHandler>();
        services.AddTransient<AcceptDocumentUploadHandler>();
        services.AddTransient<StartDocumentUploadProcessingHandler>();
        services.AddTransient<CompleteDocumentUploadHandler>();
        services.AddTransient<FailDocumentUploadHandler>();
        services.AddTransient<CancelDocumentUploadHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RequestDocumentUploadCommand, RequestDocumentUploadHandler>();
        engine.Register<AcceptDocumentUploadCommand, AcceptDocumentUploadHandler>();
        engine.Register<StartDocumentUploadProcessingCommand, StartDocumentUploadProcessingHandler>();
        engine.Register<CompleteDocumentUploadCommand, CompleteDocumentUploadHandler>();
        engine.Register<FailDocumentUploadCommand, FailDocumentUploadHandler>();
        engine.Register<CancelDocumentUploadCommand, CancelDocumentUploadHandler>();
    }
}
