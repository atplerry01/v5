using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemReconciliation.DiscrepancyResolution;

public sealed class CompleteDiscrepancyResolutionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteDiscrepancyResolutionCommand cmd)
            return;

        var aggregate = (DiscrepancyResolutionAggregate)await context.LoadAggregateAsync(typeof(DiscrepancyResolutionAggregate));
        aggregate.Complete(
            Enum.Parse<ResolutionOutcome>(cmd.Outcome, ignoreCase: true),
            cmd.Notes,
            cmd.CompletedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
