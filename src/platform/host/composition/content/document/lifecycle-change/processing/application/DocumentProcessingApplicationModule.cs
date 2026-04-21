using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.LifecycleChange.Processing.Application;

public static class DocumentProcessingApplicationModule
{
    public static IServiceCollection AddDocumentProcessingApplication(this IServiceCollection services)
    {
        services.AddTransient<RequestDocumentProcessingHandler>();
        services.AddTransient<StartDocumentProcessingHandler>();
        services.AddTransient<CompleteDocumentProcessingHandler>();
        services.AddTransient<FailDocumentProcessingHandler>();
        services.AddTransient<CancelDocumentProcessingHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RequestDocumentProcessingCommand, RequestDocumentProcessingHandler>();
        engine.Register<StartDocumentProcessingCommand, StartDocumentProcessingHandler>();
        engine.Register<CompleteDocumentProcessingCommand, CompleteDocumentProcessingHandler>();
        engine.Register<FailDocumentProcessingCommand, FailDocumentProcessingHandler>();
        engine.Register<CancelDocumentProcessingCommand, CancelDocumentProcessingHandler>();
    }
}
