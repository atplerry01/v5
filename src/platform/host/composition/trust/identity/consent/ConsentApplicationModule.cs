using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Trust.Identity.Consent;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;

namespace Whycespace.Platform.Host.Composition.Trust.Identity.Consent;

public static class ConsentApplicationModule
{
    public static IServiceCollection AddConsentApplication(this IServiceCollection services)
    {
        services.AddTransient<GrantConsentHandler>();
        services.AddTransient<RevokeConsentHandler>();
        services.AddTransient<ExpireConsentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<GrantConsentCommand, GrantConsentHandler>();
        engine.Register<RevokeConsentCommand, RevokeConsentHandler>();
        engine.Register<ExpireConsentCommand, ExpireConsentHandler>();
    }
}
