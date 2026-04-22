using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyDecision.Application;

public static class PolicyDecisionApplicationModule
{
    public static IServiceCollection AddPolicyDecisionApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordPolicyDecisionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordPolicyDecisionCommand, RecordPolicyDecisionHandler>();
    }
}
