using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Channel;

public sealed class DisableChannelHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DisableChannelCommand cmd) return;
        var aggregate = (ChannelAggregate)await context.LoadAggregateAsync(typeof(ChannelAggregate));
        aggregate.Disable(cmd.Reason, new Timestamp(cmd.DisabledAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
