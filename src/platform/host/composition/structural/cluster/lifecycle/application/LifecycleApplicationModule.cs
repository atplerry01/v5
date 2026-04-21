using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Cluster.Lifecycle;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;

namespace Whycespace.Platform.Host.Composition.Structural.Cluster.Lifecycle.Application;

public static class LifecycleApplicationModule
{
    public static IServiceCollection AddLifecycleApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineLifecycleHandler>();
        services.AddTransient<TransitionLifecycleHandler>();
        services.AddTransient<CompleteLifecycleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineLifecycleCommand, DefineLifecycleHandler>();
        engine.Register<TransitionLifecycleCommand, TransitionLifecycleHandler>();
        engine.Register<CompleteLifecycleCommand, CompleteLifecycleHandler>();
    }
}
