using Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;
using Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Constitutional.Chain.Ledger;

public sealed class OpenLedgerHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not OpenLedgerCommand cmd)
            return Task.CompletedTask;

        var aggregate = LedgerAggregate.Open(
            new Domain.ConstitutionalSystem.Chain.Ledger.LedgerId(cmd.LedgerId),
            new LedgerDescriptor(cmd.LedgerName),
            cmd.OpenedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
