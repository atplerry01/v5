using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Risk;

/// <summary>
/// E5.1 — risk context policy bindings. Registers one
/// <see cref="CommandPolicyBinding"/> per risk command, mapping the command CLR
/// type to its canonical policy id constant declared on the matching
/// <c>Risk{Domain}PolicyIds</c> class. Bindings are aggregated by
/// <see cref="ICommandPolicyIdRegistry"/> at runtime composition; once
/// registered, every dispatch of a risk command stamps the correct policy id
/// onto <c>CommandContext.PolicyId</c> for evaluation by
/// <c>PolicyMiddleware</c>.
///
/// Coverage: 4 commands → 4 unique policy ids.
/// </summary>
public static class RiskPolicyModule
{
    public static IServiceCollection AddRiskPolicyBindings(this IServiceCollection services)
    {
        // ── exposure (4) ───────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateRiskExposureCommand),   RiskExposurePolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(IncreaseRiskExposureCommand), RiskExposurePolicyIds.Increase));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReduceRiskExposureCommand),   RiskExposurePolicyIds.Reduce));
        services.AddSingleton(new CommandPolicyBinding(typeof(CloseRiskExposureCommand),    RiskExposurePolicyIds.Close));

        return services;
    }
}
