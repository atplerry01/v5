using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.Intake.Ingest.Application;

public static class MediaIngestApplicationModule
{
    public static IServiceCollection AddMediaIngestApplication(this IServiceCollection services)
    {
        services.AddTransient<RequestMediaIngestHandler>();
        services.AddTransient<AcceptMediaIngestHandler>();
        services.AddTransient<StartMediaIngestProcessingHandler>();
        services.AddTransient<CompleteMediaIngestHandler>();
        services.AddTransient<FailMediaIngestHandler>();
        services.AddTransient<CancelMediaIngestHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RequestMediaIngestCommand, RequestMediaIngestHandler>();
        engine.Register<AcceptMediaIngestCommand, AcceptMediaIngestHandler>();
        engine.Register<StartMediaIngestProcessingCommand, StartMediaIngestProcessingHandler>();
        engine.Register<CompleteMediaIngestCommand, CompleteMediaIngestHandler>();
        engine.Register<FailMediaIngestCommand, FailMediaIngestHandler>();
        engine.Register<CancelMediaIngestCommand, CancelMediaIngestHandler>();
    }
}
