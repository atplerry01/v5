using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Customer.SegmentationAndLifecycle.Lifecycle.Application;

public static class LifecycleApplicationModule
{
    public static IServiceCollection AddLifecycleApplication(this IServiceCollection services)
    {
        services.AddTransient<StartLifecycleHandler>();
        services.AddTransient<ChangeLifecycleStageHandler>();
        services.AddTransient<CloseLifecycleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<StartLifecycleCommand, StartLifecycleHandler>();
        engine.Register<ChangeLifecycleStageCommand, ChangeLifecycleStageHandler>();
        engine.Register<CloseLifecycleCommand, CloseLifecycleHandler>();
    }
}
