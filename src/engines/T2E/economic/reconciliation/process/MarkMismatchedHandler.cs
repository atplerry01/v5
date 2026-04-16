using Whycespace.Domain.EconomicSystem.Reconciliation.Process;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Reconciliation.Process;

public sealed class MarkMismatchedHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkMismatchedCommand)
            return;

        var aggregate = (ProcessAggregate)await context.LoadAggregateAsync(typeof(ProcessAggregate));
        aggregate.MarkMismatched();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
