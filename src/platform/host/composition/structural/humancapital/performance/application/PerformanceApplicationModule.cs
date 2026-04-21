using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Performance;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Performance;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Performance.Application;

public static class PerformanceApplicationModule
{
    public static IServiceCollection AddPerformanceApplication(this IServiceCollection services)
    {
        services.AddTransient<CreatePerformanceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreatePerformanceCommand, CreatePerformanceHandler>();
    }
}
