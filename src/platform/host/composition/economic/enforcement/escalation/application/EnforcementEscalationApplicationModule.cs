using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement.Escalation.Application;

public static class EnforcementEscalationApplicationModule
{
    public static IServiceCollection AddEnforcementEscalationApplication(this IServiceCollection services)
    {
        services.AddTransient<AccumulateViolationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<AccumulateViolationCommand, AccumulateViolationHandler>();
    }
}
