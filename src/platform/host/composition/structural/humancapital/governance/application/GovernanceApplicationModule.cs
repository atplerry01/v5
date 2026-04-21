using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Governance;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Governance;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Governance.Application;

public static class GovernanceApplicationModule
{
    public static IServiceCollection AddGovernanceApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateGovernanceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateGovernanceCommand, CreateGovernanceHandler>();
    }
}
