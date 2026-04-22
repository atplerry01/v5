using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.DeliveryGovernance.EntitlementHook.Application;

public static class EntitlementHookApplicationModule
{
    public static IServiceCollection AddEntitlementHookApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterEntitlementHookHandler>();
        services.AddTransient<RecordEntitlementQueryHandler>();
        services.AddTransient<RefreshEntitlementHandler>();
        services.AddTransient<InvalidateEntitlementHandler>();
        services.AddTransient<RecordEntitlementFailureHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterEntitlementHookCommand, RegisterEntitlementHookHandler>();
        engine.Register<RecordEntitlementQueryCommand, RecordEntitlementQueryHandler>();
        engine.Register<RefreshEntitlementCommand, RefreshEntitlementHandler>();
        engine.Register<InvalidateEntitlementCommand, InvalidateEntitlementHandler>();
        engine.Register<RecordEntitlementFailureCommand, RecordEntitlementFailureHandler>();
    }
}
