using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Streaming.DeliveryGovernance.Access.Application;

public static class StreamAccessApplicationModule
{
    public static IServiceCollection AddStreamAccessApplication(this IServiceCollection services)
    {
        services.AddTransient<GrantStreamAccessHandler>();
        services.AddTransient<RestrictStreamAccessHandler>();
        services.AddTransient<UnrestrictStreamAccessHandler>();
        services.AddTransient<RevokeStreamAccessHandler>();
        services.AddTransient<ExpireStreamAccessHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<GrantStreamAccessCommand, GrantStreamAccessHandler>();
        engine.Register<RestrictStreamAccessCommand, RestrictStreamAccessHandler>();
        engine.Register<UnrestrictStreamAccessCommand, UnrestrictStreamAccessHandler>();
        engine.Register<RevokeStreamAccessCommand, RevokeStreamAccessHandler>();
        engine.Register<ExpireStreamAccessCommand, ExpireStreamAccessHandler>();
    }
}
