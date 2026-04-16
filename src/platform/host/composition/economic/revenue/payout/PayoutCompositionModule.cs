using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Payout;

/// <summary>
/// Payout composition module — T2E handler DI registration and engine
/// registry binding for ExecutePayoutCommand. The workflow-based
/// PayoutExecutionWorkflowModule (Phase 2D orchestration) is separate.
/// </summary>
public static class PayoutCompositionModule
{
    public static IServiceCollection AddPayout(this IServiceCollection services)
    {
        services.AddTransient<ExecutePayoutHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<ExecutePayoutCommand, ExecutePayoutHandler>();
    }
}
