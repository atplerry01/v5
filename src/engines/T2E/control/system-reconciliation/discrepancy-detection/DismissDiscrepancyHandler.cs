using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.DiscrepancyDetection;

public sealed class DismissDiscrepancyHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DismissDiscrepancyCommand cmd)
            return;

        var aggregate = (DiscrepancyDetectionAggregate)await context.LoadAggregateAsync(typeof(DiscrepancyDetectionAggregate));
        aggregate.Dismiss(cmd.Reason, cmd.DismissedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
