using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Capital.Pool.Application;

public static class CapitalPoolApplicationModule
{
    public static IServiceCollection AddCapitalPoolApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCapitalPoolHandler>();
        services.AddTransient<AggregateCapitalToPoolHandler>();
        services.AddTransient<ReduceCapitalFromPoolHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateCapitalPoolCommand, CreateCapitalPoolHandler>();
        engine.Register<AggregateCapitalToPoolCommand, AggregateCapitalToPoolHandler>();
        engine.Register<ReduceCapitalFromPoolCommand, ReduceCapitalFromPoolHandler>();
    }
}
