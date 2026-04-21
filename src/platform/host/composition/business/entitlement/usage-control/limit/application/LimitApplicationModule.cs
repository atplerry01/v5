using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Entitlement.UsageControl.Limit.Application;

public static class LimitApplicationModule
{
    public static IServiceCollection AddLimitApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateLimitHandler>();
        services.AddTransient<EnforceLimitHandler>();
        services.AddTransient<BreachLimitHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateLimitCommand, CreateLimitHandler>();
        engine.Register<EnforceLimitCommand, EnforceLimitHandler>();
        engine.Register<BreachLimitCommand, BreachLimitHandler>();
    }
}
