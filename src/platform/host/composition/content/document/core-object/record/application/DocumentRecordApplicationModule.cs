using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Record;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.CoreObject.Record.Application;

public static class DocumentRecordApplicationModule
{
    public static IServiceCollection AddDocumentRecordApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateDocumentRecordHandler>();
        services.AddTransient<LockDocumentRecordHandler>();
        services.AddTransient<UnlockDocumentRecordHandler>();
        services.AddTransient<CloseDocumentRecordHandler>();
        services.AddTransient<ArchiveDocumentRecordHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateDocumentRecordCommand, CreateDocumentRecordHandler>();
        engine.Register<LockDocumentRecordCommand, LockDocumentRecordHandler>();
        engine.Register<UnlockDocumentRecordCommand, UnlockDocumentRecordHandler>();
        engine.Register<CloseDocumentRecordCommand, CloseDocumentRecordHandler>();
        engine.Register<ArchiveDocumentRecordCommand, ArchiveDocumentRecordHandler>();
    }
}
