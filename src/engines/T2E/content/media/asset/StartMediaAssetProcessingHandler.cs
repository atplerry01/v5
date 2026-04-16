using Whycespace.Domain.ContentSystem.Media.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Asset;

public sealed class StartMediaAssetProcessingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartMediaAssetProcessingCommand cmd)
            return;

        var aggregate = (MediaAssetAggregate)await context.LoadAggregateAsync(typeof(MediaAssetAggregate));
        aggregate.StartProcessing(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
