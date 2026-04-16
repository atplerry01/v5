using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement.Lock.Application;

public static class EnforcementLockApplicationModule
{
    public static IServiceCollection AddEnforcementLockApplication(this IServiceCollection services)
    {
        services.AddTransient<LockSystemHandler>();
        services.AddTransient<UnlockSystemHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<LockSystemCommand, LockSystemHandler>();
        engine.Register<UnlockSystemCommand, UnlockSystemHandler>();
    }
}
