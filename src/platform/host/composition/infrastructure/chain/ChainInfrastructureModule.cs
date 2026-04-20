using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Infrastructure.Chain;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Chain;

/// <summary>
/// Chain anchor capability — Postgres-backed chain anchor registration.
/// </summary>
public static class ChainInfrastructureModule
{
    public static IServiceCollection AddChain(this IServiceCollection services)
    {
        // R2.A.D.3a / R-CHAIN-BREAKER-DELEGATION-01 + R2.A.D.4 /
        // R-BREAKER-REGISTRY-01: dedicated ICircuitBreaker for the chain
        // anchor dependency. Name "chain-anchor" is the stable metrics
        // tag + health-posture reason key. Registered as a plain
        // ICircuitBreaker enumerable contributor so CircuitBreakerRegistry
        // picks it up alongside OPA (and future Kafka / Postgres / Redis
        // breakers in R2.A.D.3b/c). WhyceChainPostgresAdapter consumes it
        // via sp.GetRequiredService<ICircuitBreakerRegistry>().Get("chain-anchor").
        services.AddSingleton<ICircuitBreaker>(sp =>
        {
            var options = sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Admission.ChainAnchorOptions>();
            return new DeterministicCircuitBreaker(
                new CircuitBreakerOptions
                {
                    Name = "chain-anchor",
                    FailureThreshold = options.BreakerThreshold,
                    WindowSeconds = options.BreakerWindowSeconds
                },
                sp.GetRequiredService<IClock>());
        });

        // phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
        // register the concrete WhyceChainPostgresAdapter singleton
        // first, then forward IChainAnchor to it. Post-R2.A.D.4 the
        // RuntimeStateAggregator reads chain breaker state via the
        // registry, not the concrete getter — but the singleton
        // registration shape is preserved for any remaining consumers.
        services.AddSingleton<WhyceChainPostgresAdapter>(sp =>
            new WhyceChainPostgresAdapter(
                sp.GetRequiredService<ChainDataSource>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Admission.ChainAnchorOptions>(),
                sp.GetRequiredService<ICircuitBreakerRegistry>().Get("chain-anchor")));
        services.AddSingleton<IChainAnchor>(sp => sp.GetRequiredService<WhyceChainPostgresAdapter>());

        return services;
    }
}
