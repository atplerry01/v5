using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Archive;

public sealed class ArchiveArchiveHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveArchiveCommand cmd) return;
        var aggregate = (ArchiveAggregate)await context.LoadAggregateAsync(typeof(ArchiveAggregate));
        aggregate.Archive(new Timestamp(cmd.ArchivedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
