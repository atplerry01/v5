using Whycespace.Domain.EconomicSystem.Vault.Account;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Vault.Account;

public sealed class ApplyRevenueHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ApplyRevenueCommand cmd)
            return;

        var aggregate = (VaultAccountAggregate)await context.LoadAggregateAsync(typeof(VaultAccountAggregate));
        aggregate.ApplyRevenue(cmd.Amount, cmd.Currency);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
