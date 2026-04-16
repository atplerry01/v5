using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Vault.Account.Policy;

/// <summary>
/// E5 — vault/account context policy bindings. Registers one
/// <see cref="CommandPolicyBinding"/> per vault account command, mapping the
/// command CLR type to its canonical policy id constant declared on
/// <see cref="VaultAccountPolicyIds"/>. Bindings are aggregated by
/// <c>ICommandPolicyIdRegistry</c>; once registered, every dispatch of a
/// vault account command stamps the correct policy id onto
/// <c>CommandContext.PolicyId</c> for evaluation by <c>PolicyMiddleware</c>.
///
/// Coverage: 6 commands -> 6 unique policy ids.
/// Backing rego package: <c>infrastructure/policy/domain/economic/vault/account.rego</c>.
/// </summary>
public static class VaultAccountPolicyModule
{
    public static IServiceCollection AddVaultAccountPolicyBindings(this IServiceCollection services)
    {
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateVaultAccountCommand), VaultAccountPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(FundVaultCommand),          VaultAccountPolicyIds.Fund));
        services.AddSingleton(new CommandPolicyBinding(typeof(InvestCommand),             VaultAccountPolicyIds.Invest));
        services.AddSingleton(new CommandPolicyBinding(typeof(ApplyRevenueCommand),       VaultAccountPolicyIds.ApplyRevenue));
        services.AddSingleton(new CommandPolicyBinding(typeof(DebitSliceCommand),         VaultAccountPolicyIds.Debit));
        services.AddSingleton(new CommandPolicyBinding(typeof(CreditSliceCommand),        VaultAccountPolicyIds.Credit));

        return services;
    }
}
