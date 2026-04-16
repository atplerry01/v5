using Whycespace.Domain.EconomicSystem.Reconciliation.Process;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Reconciliation.Process;

public sealed class MarkMatchedHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkMatchedCommand)
            return;

        var aggregate = (ProcessAggregate)await context.LoadAggregateAsync(typeof(ProcessAggregate));
        aggregate.MarkMatched();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
