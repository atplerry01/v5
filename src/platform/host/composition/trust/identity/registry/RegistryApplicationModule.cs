using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;

namespace Whycespace.Platform.Host.Composition.Trust.Identity.Registry;

public static class RegistryApplicationModule
{
    public static IServiceCollection AddRegistryApplication(this IServiceCollection services)
    {
        services.AddTransient<InitiateRegistrationHandler>();
        services.AddTransient<VerifyRegistrationHandler>();
        services.AddTransient<ActivateRegistrationHandler>();
        services.AddTransient<RejectRegistrationHandler>();
        services.AddTransient<LockRegistrationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<InitiateRegistrationCommand, InitiateRegistrationHandler>();
        engine.Register<VerifyRegistrationCommand, VerifyRegistrationHandler>();
        engine.Register<ActivateRegistrationCommand, ActivateRegistrationHandler>();
        engine.Register<RejectRegistrationCommand, RejectRegistrationHandler>();
        engine.Register<LockRegistrationCommand, LockRegistrationHandler>();
    }
}
