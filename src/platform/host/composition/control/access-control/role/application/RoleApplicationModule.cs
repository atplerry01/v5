using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.AccessControl.Role.Application;

public static class RoleApplicationModule
{
    public static IServiceCollection AddRoleApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineRoleHandler>();
        services.AddTransient<AddRolePermissionHandler>();
        services.AddTransient<DeprecateRoleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineRoleCommand, DefineRoleHandler>();
        engine.Register<AddRolePermissionCommand, AddRolePermissionHandler>();
        engine.Register<DeprecateRoleCommand, DeprecateRoleHandler>();
    }
}
