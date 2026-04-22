using Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.ReconciliationRun;

public sealed class StartReconciliationRunHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartReconciliationRunCommand cmd)
            return;

        var aggregate = (ReconciliationRunAggregate)await context.LoadAggregateAsync(typeof(ReconciliationRunAggregate));
        aggregate.Start(cmd.StartedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
