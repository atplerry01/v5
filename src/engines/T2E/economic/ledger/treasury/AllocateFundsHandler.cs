using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Ledger.Treasury;

public sealed class AllocateFundsHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AllocateFundsCommand cmd)
            return;

        var treasury = (TreasuryAggregate)await context.LoadAggregateAsync(typeof(TreasuryAggregate));
        treasury.AllocateFunds(new Amount(cmd.Amount));

        context.EmitEvents(treasury.DomainEvents);
    }
}
