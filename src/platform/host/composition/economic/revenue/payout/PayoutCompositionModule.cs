using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Payout;

/// <summary>
/// Payout composition module — T2E handler DI registration and engine
/// registry binding for the payout aggregate command surface
/// (Execute / MarkExecuted / MarkFailed, plus the Phase 7 B2 compensation
/// surface RequestPayoutCompensation / MarkPayoutCompensated).
///
/// Workflow-specific modules (PayoutExecutionWorkflowModule,
/// PayoutCompensationWorkflowModule) register the T1M step bindings
/// separately.
/// </summary>
public static class PayoutCompositionModule
{
    public static IServiceCollection AddPayout(this IServiceCollection services)
    {
        services.AddTransient<ExecutePayoutHandler>();
        services.AddTransient<MarkPayoutExecutedHandler>();
        services.AddTransient<MarkPayoutFailedHandler>();
        services.AddTransient<RequestPayoutCompensationHandler>();
        services.AddTransient<MarkPayoutCompensatedHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<ExecutePayoutCommand, ExecutePayoutHandler>();
        engine.Register<MarkPayoutExecutedCommand, MarkPayoutExecutedHandler>();
        engine.Register<MarkPayoutFailedCommand, MarkPayoutFailedHandler>();
        engine.Register<RequestPayoutCompensationCommand, RequestPayoutCompensationHandler>();
        engine.Register<MarkPayoutCompensatedCommand, MarkPayoutCompensatedHandler>();
    }
}
