using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyEnforcement.Application;

public static class PolicyEnforcementApplicationModule
{
    public static IServiceCollection AddPolicyEnforcementApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordPolicyEnforcementHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordPolicyEnforcementCommand, RecordPolicyEnforcementHandler>();
    }
}
