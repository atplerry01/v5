using Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Reconciliation.Discrepancy;

public sealed class ResolveDiscrepancyHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResolveDiscrepancyCommand cmd)
            return;

        var aggregate = (DiscrepancyAggregate)await context.LoadAggregateAsync(typeof(DiscrepancyAggregate));
        aggregate.Resolve(cmd.Resolution);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
