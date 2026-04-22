using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Command.CommandRouting;
using Whycespace.Shared.Contracts.Platform.Command.CommandRouting;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Command.CommandRouting.Application;

public static class CommandRoutingApplicationModule
{
    public static IServiceCollection AddCommandRoutingApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterCommandRoutingRuleHandler>();
        services.AddTransient<RemoveCommandRoutingRuleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterCommandRoutingRuleCommand, RegisterCommandRoutingRuleHandler>();
        engine.Register<RemoveCommandRoutingRuleCommand, RemoveCommandRoutingRuleHandler>();
    }
}
