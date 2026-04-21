using Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Descriptor.Metadata;

public sealed class UpdateMediaMetadataEntryHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateMediaMetadataEntryCommand cmd) return;
        var aggregate = (MediaMetadataAggregate)await context.LoadAggregateAsync(typeof(MediaMetadataAggregate));
        aggregate.UpdateEntry(
            new MediaMetadataKey(cmd.Key),
            new MediaMetadataValue(cmd.NewValue),
            new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
