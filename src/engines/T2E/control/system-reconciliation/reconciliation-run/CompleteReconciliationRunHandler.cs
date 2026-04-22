using Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.ReconciliationRun;

public sealed class CompleteReconciliationRunHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteReconciliationRunCommand cmd)
            return;

        var aggregate = (ReconciliationRunAggregate)await context.LoadAggregateAsync(typeof(ReconciliationRunAggregate));
        aggregate.Complete(cmd.ChecksProcessed, cmd.DiscrepanciesFound, cmd.CompletedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
