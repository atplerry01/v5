using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Control.AccessControl.Authorization;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.AccessControl.Authorization.Application;

public static class AuthorizationApplicationModule
{
    public static IServiceCollection AddAuthorizationApplication(this IServiceCollection services)
    {
        services.AddTransient<GrantAuthorizationHandler>();
        services.AddTransient<RevokeAuthorizationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<GrantAuthorizationCommand, GrantAuthorizationHandler>();
        engine.Register<RevokeAuthorizationCommand, RevokeAuthorizationHandler>();
    }
}
