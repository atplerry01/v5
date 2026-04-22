using Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.ConsistencyCheck;

public sealed class CompleteConsistencyCheckHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteConsistencyCheckCommand cmd)
            return;

        var aggregate = (ConsistencyCheckAggregate)await context.LoadAggregateAsync(typeof(ConsistencyCheckAggregate));
        aggregate.Complete(cmd.HasDiscrepancies, cmd.CompletedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
