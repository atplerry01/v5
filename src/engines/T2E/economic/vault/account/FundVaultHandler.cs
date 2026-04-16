using Whycespace.Domain.EconomicSystem.Vault.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Vault.Account;

public sealed class FundVaultHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FundVaultCommand cmd)
            return;

        var aggregate = (VaultAccountAggregate)await context.LoadAggregateAsync(typeof(VaultAccountAggregate));
        aggregate.Fund(new Amount(cmd.Amount), new Currency(cmd.Currency));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
