using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.AccessControl.Principal.Application;

public static class PrincipalApplicationModule
{
    public static IServiceCollection AddPrincipalApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterPrincipalHandler>();
        services.AddTransient<AssignPrincipalRoleHandler>();
        services.AddTransient<DeactivatePrincipalHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterPrincipalCommand, RegisterPrincipalHandler>();
        engine.Register<AssignPrincipalRoleCommand, AssignPrincipalRoleHandler>();
        engine.Register<DeactivatePrincipalCommand, DeactivatePrincipalHandler>();
    }
}
