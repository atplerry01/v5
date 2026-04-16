using Whycespace.Domain.ContentSystem.Media.Asset;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Asset;

public sealed class UpdateMediaAssetMetadataHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateMediaAssetMetadataCommand cmd)
            return;

        var aggregate = (MediaAssetAggregate)await context.LoadAggregateAsync(typeof(MediaAssetAggregate));
        var tags = cmd.Tags is null
            ? Array.Empty<MediaTag>()
            : cmd.Tags.Select(MediaTag.Create).ToArray();

        aggregate.UpdateMetadata(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            MediaTitle.Create(cmd.Title),
            MediaDescription.Create(cmd.Description),
            tags,
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
