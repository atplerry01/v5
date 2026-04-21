using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Entitlement.EligibilityAndGrant.Grant.Application;

public static class GrantApplicationModule
{
    public static IServiceCollection AddGrantApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateGrantHandler>();
        services.AddTransient<ActivateGrantHandler>();
        services.AddTransient<RevokeGrantHandler>();
        services.AddTransient<ExpireGrantHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateGrantCommand, CreateGrantHandler>();
        engine.Register<ActivateGrantCommand, ActivateGrantHandler>();
        engine.Register<RevokeGrantCommand, RevokeGrantHandler>();
        engine.Register<ExpireGrantCommand, ExpireGrantHandler>();
    }
}
