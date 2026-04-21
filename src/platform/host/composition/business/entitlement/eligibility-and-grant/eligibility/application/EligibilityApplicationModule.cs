using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Entitlement.EligibilityAndGrant.Eligibility.Application;

public static class EligibilityApplicationModule
{
    public static IServiceCollection AddEligibilityApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateEligibilityHandler>();
        services.AddTransient<MarkEligibleHandler>();
        services.AddTransient<MarkIneligibleHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateEligibilityCommand, CreateEligibilityHandler>();
        engine.Register<MarkEligibleCommand, MarkEligibleHandler>();
        engine.Register<MarkIneligibleCommand, MarkIneligibleHandler>();
    }
}
