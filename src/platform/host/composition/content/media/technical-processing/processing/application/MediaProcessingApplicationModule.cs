using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Media.TechnicalProcessing.Processing.Application;

public static class MediaProcessingApplicationModule
{
    public static IServiceCollection AddMediaProcessingApplication(this IServiceCollection services)
    {
        services.AddTransient<RequestMediaProcessingHandler>();
        services.AddTransient<StartMediaProcessingHandler>();
        services.AddTransient<CompleteMediaProcessingHandler>();
        services.AddTransient<FailMediaProcessingHandler>();
        services.AddTransient<CancelMediaProcessingHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RequestMediaProcessingCommand, RequestMediaProcessingHandler>();
        engine.Register<StartMediaProcessingCommand, StartMediaProcessingHandler>();
        engine.Register<CompleteMediaProcessingCommand, CompleteMediaProcessingHandler>();
        engine.Register<FailMediaProcessingCommand, FailMediaProcessingHandler>();
        engine.Register<CancelMediaProcessingCommand, CancelMediaProcessingHandler>();
    }
}
