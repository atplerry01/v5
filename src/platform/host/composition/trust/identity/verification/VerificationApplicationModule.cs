using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Trust.Identity.Verification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;

namespace Whycespace.Platform.Host.Composition.Trust.Identity.Verification;

public static class VerificationApplicationModule
{
    public static IServiceCollection AddVerificationApplication(this IServiceCollection services)
    {
        services.AddTransient<InitiateVerificationHandler>();
        services.AddTransient<PassVerificationHandler>();
        services.AddTransient<FailVerificationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<InitiateVerificationCommand, InitiateVerificationHandler>();
        engine.Register<PassVerificationCommand, PassVerificationHandler>();
        engine.Register<FailVerificationCommand, FailVerificationHandler>();
    }
}
