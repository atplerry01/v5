using Whycespace.Domain.EconomicSystem.Capital.Vault;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Vault;

public sealed class DepositToCapitalVaultSliceHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DepositToCapitalVaultSliceCommand cmd)
            return;

        var aggregate = (VaultAggregate)await context.LoadAggregateAsync(typeof(VaultAggregate));
        aggregate.DepositToSlice(new SliceId(cmd.SliceId), new Amount(cmd.Amount));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
