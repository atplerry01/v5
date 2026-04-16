using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement.Rule.Application;

public static class EnforcementRuleApplicationModule
{
    public static IServiceCollection AddEnforcementRuleApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineEnforcementRuleHandler>();
        services.AddTransient<ActivateEnforcementRuleHandler>();
        services.AddTransient<DisableEnforcementRuleHandler>();
        services.AddTransient<RetireEnforcementRuleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineEnforcementRuleCommand, DefineEnforcementRuleHandler>();
        engine.Register<ActivateEnforcementRuleCommand, ActivateEnforcementRuleHandler>();
        engine.Register<DisableEnforcementRuleCommand, DisableEnforcementRuleHandler>();
        engine.Register<RetireEnforcementRuleCommand, RetireEnforcementRuleHandler>();
    }
}
