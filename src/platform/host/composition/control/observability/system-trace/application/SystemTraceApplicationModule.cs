using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Control.Observability.SystemTrace;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Observability.SystemTrace.Application;

public static class SystemTraceApplicationModule
{
    public static IServiceCollection AddSystemTraceApplication(this IServiceCollection services)
    {
        services.AddTransient<StartSystemTraceHandler>();
        services.AddTransient<CompleteSystemTraceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<StartSystemTraceCommand, StartSystemTraceHandler>();
        engine.Register<CompleteSystemTraceCommand, CompleteSystemTraceHandler>();
    }
}
