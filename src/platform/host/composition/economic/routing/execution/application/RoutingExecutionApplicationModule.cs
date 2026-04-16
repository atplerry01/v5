using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Routing.Execution.Application;

public static class RoutingExecutionApplicationModule
{
    public static IServiceCollection AddRoutingExecutionApplication(this IServiceCollection services)
    {
        services.AddTransient<StartExecutionHandler>();
        services.AddTransient<CompleteExecutionHandler>();
        services.AddTransient<FailExecutionHandler>();
        services.AddTransient<AbortExecutionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<StartExecutionCommand,    StartExecutionHandler>();
        engine.Register<CompleteExecutionCommand, CompleteExecutionHandler>();
        engine.Register<FailExecutionCommand,     FailExecutionHandler>();
        engine.Register<AbortExecutionCommand,    AbortExecutionHandler>();
    }
}
