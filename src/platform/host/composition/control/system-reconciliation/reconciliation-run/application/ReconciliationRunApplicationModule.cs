using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemReconciliation.ReconciliationRun.Application;

public static class ReconciliationRunApplicationModule
{
    public static IServiceCollection AddReconciliationRunApplication(this IServiceCollection services)
    {
        services.AddTransient<ScheduleReconciliationRunHandler>();
        services.AddTransient<StartReconciliationRunHandler>();
        services.AddTransient<CompleteReconciliationRunHandler>();
        services.AddTransient<AbortReconciliationRunHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<ScheduleReconciliationRunCommand, ScheduleReconciliationRunHandler>();
        engine.Register<StartReconciliationRunCommand, StartReconciliationRunHandler>();
        engine.Register<CompleteReconciliationRunCommand, CompleteReconciliationRunHandler>();
        engine.Register<AbortReconciliationRunCommand, AbortReconciliationRunHandler>();
    }
}
