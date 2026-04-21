using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Channel;

public sealed class EnableChannelHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EnableChannelCommand cmd) return;
        var aggregate = (ChannelAggregate)await context.LoadAggregateAsync(typeof(ChannelAggregate));
        aggregate.Enable(new Timestamp(cmd.EnabledAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
