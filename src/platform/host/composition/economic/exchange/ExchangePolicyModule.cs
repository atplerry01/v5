using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Exchange;

/// <summary>
/// Exchange context policy bindings. Registers one <see cref="CommandPolicyBinding"/>
/// per exchange command, mapping the command CLR type to its canonical policy id
/// constant declared on <c>FxPolicyIds</c> / <c>ExchangeRatePolicyIds</c>.
/// Coverage: 6 commands → 6 unique policy ids.
/// </summary>
public static class ExchangePolicyModule
{
    public static IServiceCollection AddExchangePolicyBindings(this IServiceCollection services)
    {
        // ── fx (3) ────────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(RegisterFxPairCommand),   FxPolicyIds.Register));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateFxPairCommand),   FxPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(DeactivateFxPairCommand), FxPolicyIds.Deactivate));

        // ── rate (3) ──────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(DefineExchangeRateCommand),   ExchangeRatePolicyIds.Define));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateExchangeRateCommand), ExchangeRatePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireExchangeRateCommand),   ExchangeRatePolicyIds.Expire));

        return services;
    }
}
