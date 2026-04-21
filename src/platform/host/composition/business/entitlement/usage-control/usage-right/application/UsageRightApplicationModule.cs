using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Entitlement.UsageControl.UsageRight.Application;

public static class UsageRightApplicationModule
{
    public static IServiceCollection AddUsageRightApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateUsageRightHandler>();
        services.AddTransient<UseUsageRightHandler>();
        services.AddTransient<ConsumeUsageRightHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateUsageRightCommand, CreateUsageRightHandler>();
        engine.Register<UseUsageRightCommand, UseUsageRightHandler>();
        engine.Register<ConsumeUsageRightCommand, ConsumeUsageRightHandler>();
    }
}
