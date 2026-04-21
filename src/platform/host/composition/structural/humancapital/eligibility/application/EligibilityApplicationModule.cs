using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Eligibility;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Eligibility;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Eligibility.Application;

public static class EligibilityApplicationModule
{
    public static IServiceCollection AddEligibilityApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateEligibilityHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateEligibilityCommand, CreateEligibilityHandler>();
    }
}
