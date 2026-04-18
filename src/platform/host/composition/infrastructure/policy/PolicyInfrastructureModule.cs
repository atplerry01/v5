using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.Middleware.Policy;
using Whycespace.Runtime.Middleware.Policy.Loaders;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Policy;

/// <summary>
/// Policy evaluator capability — OPA configuration and evaluator registration.
/// </summary>
public static class PolicyInfrastructureModule
{
    public static IServiceCollection AddPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var opaEndpoint = configuration.GetValue<string>("OPA:Endpoint")
            ?? throw new InvalidOperationException("OPA:Endpoint is required. No fallback.");

        // phase1.5-S5.2.1 / PC-2 (OPA-CONFIG-01): bind OpaOptions from
        // configuration following the phase1.6-S1.5 OutboxOptions
        // precedent — a plain record constructed at the composition root,
        // no IOptions<T> indirection. The HttpClient.Timeout is left at
        // the .NET default; the per-call envelope is enforced by the
        // evaluator's linked CTS sized from OpaOptions.RequestTimeoutMs.
        var opaOptions = new OpaOptions
        {
            Endpoint = opaEndpoint,
            RequestTimeoutMs = configuration.GetValue<int?>("Opa:RequestTimeoutMs")
                ?? new OpaOptions().RequestTimeoutMs,
            BreakerThreshold = configuration.GetValue<int?>("Opa:BreakerThreshold")
                ?? new OpaOptions().BreakerThreshold,
            BreakerWindowSeconds = configuration.GetValue<int?>("Opa:BreakerWindowSeconds")
                ?? new OpaOptions().BreakerWindowSeconds,
            OpenStateBehavior = configuration.GetValue<string>("Opa:OpenStateBehavior")
                ?? new OpaOptions().OpenStateBehavior,
        };
        services.AddSingleton(opaOptions);
        // phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
        // mirror of the chain-anchor pattern above. Register the
        // concrete OpaPolicyEvaluator first so RuntimeStateAggregator
        // can read its side-effect-free IsBreakerOpen getter; then
        // forward IPolicyEvaluator to the same singleton.
        services.AddSingleton<OpaPolicyEvaluator>(sp =>
            new OpaPolicyEvaluator(
                new HttpClient(),
                sp.GetRequiredService<OpaOptions>(),
                sp.GetRequiredService<IClock>()));
        services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<OpaPolicyEvaluator>());

        // Phase 11 B3 — CompositeAggregateStateLoader replaces the
        // Phase 8 B6 NullAggregateStateLoader default. Routes per-command
        // to the appropriate per-aggregate loader; unregistered command
        // types still fall through to null, so the backward-compat
        // behaviour (pre-Phase 11 allow/deny) is preserved for every
        // aggregate without a loader.
        //
        // Event-store fidelity per POLICY-STATE-SOURCE-EVENT-STORE-01:
        // each per-aggregate loader receives the same IEventStore the
        // engine uses — never a projection.
        services.AddSingleton<IAggregateStateLoader>(sp =>
        {
            var eventStore = sp.GetRequiredService<IEventStore>();

            return new CompositeAggregateStateLoader(new[]
            {
                new CompositeAggregateStateLoader.Route(
                    ObligationStateLoader.Handles,
                    new ObligationStateLoader(eventStore)),

                new CompositeAggregateStateLoader.Route(
                    TreasuryStateLoader.Handles,
                    new TreasuryStateLoader(eventStore)),
            });
        });

        // Phase 2.6 hardening: fire a single best-effort warm-up ping at
        // OPA during host startup so the first real policy evaluation
        // does not pay the cold-start cost. Non-blocking, never throws.
        services.AddHostedService<OpaWarmupService>();

        return services;
    }
}
