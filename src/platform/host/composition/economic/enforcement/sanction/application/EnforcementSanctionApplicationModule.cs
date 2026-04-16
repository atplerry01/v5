using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement.Sanction.Application;

public static class EnforcementSanctionApplicationModule
{
    public static IServiceCollection AddEnforcementSanctionApplication(this IServiceCollection services)
    {
        services.AddTransient<IssueSanctionHandler>();
        services.AddTransient<ActivateSanctionHandler>();
        services.AddTransient<RevokeSanctionHandler>();
        services.AddTransient<ExpireSanctionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<IssueSanctionCommand, IssueSanctionHandler>();
        engine.Register<ActivateSanctionCommand, ActivateSanctionHandler>();
        engine.Register<RevokeSanctionCommand, RevokeSanctionHandler>();
        engine.Register<ExpireSanctionCommand, ExpireSanctionHandler>();
    }
}
