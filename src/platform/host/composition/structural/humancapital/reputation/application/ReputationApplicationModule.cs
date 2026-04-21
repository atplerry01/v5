using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Reputation;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Reputation;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Reputation.Application;

public static class ReputationApplicationModule
{
    public static IServiceCollection AddReputationApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateReputationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateReputationCommand, CreateReputationHandler>();
    }
}
