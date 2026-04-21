using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Manifest;

public sealed class ArchiveManifestHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveManifestCommand cmd) return;
        var aggregate = (ManifestAggregate)await context.LoadAggregateAsync(typeof(ManifestAggregate));
        aggregate.Archive(new Timestamp(cmd.ArchivedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
