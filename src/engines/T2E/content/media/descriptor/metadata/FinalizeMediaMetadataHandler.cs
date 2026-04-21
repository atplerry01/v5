using Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Descriptor.Metadata;

public sealed class FinalizeMediaMetadataHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FinalizeMediaMetadataCommand cmd) return;
        var aggregate = (MediaMetadataAggregate)await context.LoadAggregateAsync(typeof(MediaMetadataAggregate));
        aggregate.Finalize(new Timestamp(cmd.FinalizedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
