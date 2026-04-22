using Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;
using Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Constitutional.Chain.Ledger;

public sealed class SealLedgerHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SealLedgerCommand cmd)
            return;

        var aggregate = (LedgerAggregate)await context.LoadAggregateAsync(typeof(LedgerAggregate));
        aggregate.Seal(cmd.SealedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
