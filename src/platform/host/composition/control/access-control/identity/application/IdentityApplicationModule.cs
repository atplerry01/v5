using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.AccessControl.Identity.Application;

public static class IdentityApplicationModule
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterIdentityHandler>();
        services.AddTransient<SuspendIdentityHandler>();
        services.AddTransient<DeactivateIdentityHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterIdentityCommand, RegisterIdentityHandler>();
        engine.Register<SuspendIdentityCommand, SuspendIdentityHandler>();
        engine.Register<DeactivateIdentityCommand, DeactivateIdentityHandler>();
    }
}
