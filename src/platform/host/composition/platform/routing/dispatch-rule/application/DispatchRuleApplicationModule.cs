using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Routing.DispatchRule;
using Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Routing.DispatchRule.Application;

public static class DispatchRuleApplicationModule
{
    public static IServiceCollection AddDispatchRuleApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterDispatchRuleHandler>();
        services.AddTransient<DeactivateDispatchRuleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterDispatchRuleCommand, RegisterDispatchRuleHandler>();
        engine.Register<DeactivateDispatchRuleCommand, DeactivateDispatchRuleHandler>();
    }
}
