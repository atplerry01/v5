using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue;

/// <summary>
/// Revenue context policy bindings. Registers one
/// <see cref="CommandPolicyBinding"/> per revenue command, mapping the command
/// CLR type to its canonical policy id constant. The bindings are aggregated
/// by <see cref="ICommandPolicyIdRegistry"/> at runtime; once registered,
/// every dispatch of a revenue command stamps the correct policy id onto
/// <c>CommandContext.PolicyId</c> for evaluation by <c>PolicyMiddleware</c>.
///
/// Coverage: 8 commands across 5 subdomains (contract, distribution, payout,
/// pricing, revenue).
/// </summary>
public static class RevenuePolicyModule
{
    public static IServiceCollection AddRevenuePolicyBindings(this IServiceCollection services)
    {
        // ── contract (3) ──────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateRevenueContractCommand),    ContractPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateRevenueContractCommand),  ContractPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(TerminateRevenueContractCommand), ContractPolicyIds.Terminate));

        // ── distribution (4) ──────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateDistributionCommand),     DistributionPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(ConfirmDistributionCommand),    DistributionPolicyIds.Confirm));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkDistributionPaidCommand),   DistributionPolicyIds.MarkPaid));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkDistributionFailedCommand), DistributionPolicyIds.MarkFailed));

        // ── payout (3) ────────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(ExecutePayoutCommand),      PayoutPolicyIds.Execute));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkPayoutExecutedCommand), PayoutPolicyIds.MarkExecuted));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkPayoutFailedCommand),   PayoutPolicyIds.MarkFailed));

        // ── pricing (2) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(DefinePricingCommand), PricingPolicyIds.Define));
        services.AddSingleton(new CommandPolicyBinding(typeof(AdjustPricingCommand), PricingPolicyIds.Adjust));

        // ── revenue (1) ───────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(RecordRevenueCommand), RevenuePolicyIds.Record));

        return services;
    }
}
