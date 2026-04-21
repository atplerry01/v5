using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Incentive;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Incentive;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Incentive.Application;

public static class IncentiveApplicationModule
{
    public static IServiceCollection AddIncentiveApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateIncentiveHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateIncentiveCommand, CreateIncentiveHandler>();
    }
}
