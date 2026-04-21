using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Availability;

public sealed class ArchivePlaybackHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchivePlaybackCommand cmd) return;
        var aggregate = (PlaybackAggregate)await context.LoadAggregateAsync(typeof(PlaybackAggregate));
        aggregate.Archive(new Timestamp(cmd.ArchivedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
