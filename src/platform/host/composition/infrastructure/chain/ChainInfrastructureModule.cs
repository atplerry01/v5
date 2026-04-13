using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Shared.Contracts.Infrastructure.Chain;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Chain;

/// <summary>
/// Chain anchor capability — Postgres-backed chain anchor registration.
/// </summary>
public static class ChainInfrastructureModule
{
    public static IServiceCollection AddChain(this IServiceCollection services)
    {
        // phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
        // register the concrete WhyceChainPostgresAdapter singleton
        // first, then forward IChainAnchor to it. RuntimeStateAggregator
        // depends on the concrete type so it can read the new
        // side-effect-free IsBreakerOpen getter — adding the getter
        // to IChainAnchor would widen scope into the TC-3 contract.
        services.AddSingleton<WhyceChainPostgresAdapter>(sp =>
            new WhyceChainPostgresAdapter(
                sp.GetRequiredService<ChainDataSource>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Admission.ChainAnchorOptions>()));
        services.AddSingleton<IChainAnchor>(sp => sp.GetRequiredService<WhyceChainPostgresAdapter>());

        return services;
    }
}
