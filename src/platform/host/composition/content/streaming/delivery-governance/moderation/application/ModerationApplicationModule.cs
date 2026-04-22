using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.DeliveryGovernance.Moderation.Application;

public static class ModerationApplicationModule
{
    public static IServiceCollection AddModerationApplication(this IServiceCollection services)
    {
        services.AddTransient<FlagStreamHandler>();
        services.AddTransient<AssignModerationHandler>();
        services.AddTransient<DecideModerationHandler>();
        services.AddTransient<OverturnModerationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<FlagStreamCommand, FlagStreamHandler>();
        engine.Register<AssignModerationCommand, AssignModerationHandler>();
        engine.Register<DecideModerationCommand, DecideModerationHandler>();
        engine.Register<OverturnModerationCommand, OverturnModerationHandler>();
    }
}
