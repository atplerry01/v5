using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Observability;

public sealed class FinalizeObservabilityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FinalizeObservabilityCommand cmd) return;
        var aggregate = (ObservabilityAggregate)await context.LoadAggregateAsync(typeof(ObservabilityAggregate));
        aggregate.Finalize(new Timestamp(cmd.FinalizedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
