using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Platform.Host.Adapters;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Composition.Infrastructure.Policy;

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

        return services;
    }
}
