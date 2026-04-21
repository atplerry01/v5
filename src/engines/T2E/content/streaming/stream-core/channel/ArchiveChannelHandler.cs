using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Channel;

public sealed class ArchiveChannelHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveChannelCommand cmd) return;
        var aggregate = (ChannelAggregate)await context.LoadAggregateAsync(typeof(ChannelAggregate));
        aggregate.Archive(new Timestamp(cmd.ArchivedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
