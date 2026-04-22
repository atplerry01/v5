using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.AccessControl.Permission.Application;

public static class PermissionApplicationModule
{
    public static IServiceCollection AddPermissionApplication(this IServiceCollection services)
    {
        services.AddTransient<DefinePermissionHandler>();
        services.AddTransient<DeprecatePermissionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefinePermissionCommand, DefinePermissionHandler>();
        engine.Register<DeprecatePermissionCommand, DeprecatePermissionHandler>();
    }
}
