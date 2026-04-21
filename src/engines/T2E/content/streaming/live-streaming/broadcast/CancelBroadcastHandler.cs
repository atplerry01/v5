using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Broadcast;

public sealed class CancelBroadcastHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CancelBroadcastCommand cmd) return;
        var aggregate = (BroadcastAggregate)await context.LoadAggregateAsync(typeof(BroadcastAggregate));
        aggregate.Cancel(cmd.Reason, new Timestamp(cmd.CancelledAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
