using Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Descriptor.Metadata;

public sealed class CreateMediaMetadataHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateMediaMetadataCommand cmd) return Task.CompletedTask;
        var aggregate = MediaMetadataAggregate.Create(
            new MediaMetadataId(cmd.MetadataId),
            new MediaAssetRef(cmd.AssetRef),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
