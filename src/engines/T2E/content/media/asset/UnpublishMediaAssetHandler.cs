using Whycespace.Domain.ContentSystem.Media.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Asset;

public sealed class UnpublishMediaAssetHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UnpublishMediaAssetCommand cmd)
            return;

        var aggregate = (MediaAssetAggregate)await context.LoadAggregateAsync(typeof(MediaAssetAggregate));
        aggregate.Unpublish(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
