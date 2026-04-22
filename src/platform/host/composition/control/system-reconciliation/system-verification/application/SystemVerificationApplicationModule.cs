using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemReconciliation.SystemVerification.Application;

public static class SystemVerificationApplicationModule
{
    public static IServiceCollection AddSystemVerificationApplication(this IServiceCollection services)
    {
        services.AddTransient<InitiateSystemVerificationHandler>();
        services.AddTransient<PassSystemVerificationHandler>();
        services.AddTransient<FailSystemVerificationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<InitiateSystemVerificationCommand, InitiateSystemVerificationHandler>();
        engine.Register<PassSystemVerificationCommand, PassSystemVerificationHandler>();
        engine.Register<FailSystemVerificationCommand, FailSystemVerificationHandler>();
    }
}
