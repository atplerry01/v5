using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Reconciliation.Process.Application;

public static class ReconciliationProcessApplicationModule
{
    public static IServiceCollection AddReconciliationProcessApplication(this IServiceCollection services)
    {
        services.AddTransient<TriggerReconciliationHandler>();
        services.AddTransient<MarkMatchedHandler>();
        services.AddTransient<MarkMismatchedHandler>();
        services.AddTransient<ResolveReconciliationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<TriggerReconciliationCommand, TriggerReconciliationHandler>();
        engine.Register<MarkMatchedCommand,           MarkMatchedHandler>();
        engine.Register<MarkMismatchedCommand,        MarkMismatchedHandler>();
        engine.Register<ResolveReconciliationCommand, ResolveReconciliationHandler>();
    }
}
