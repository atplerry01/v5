using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Broadcast;

public sealed class ResumeBroadcastHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeBroadcastCommand cmd) return;
        var aggregate = (BroadcastAggregate)await context.LoadAggregateAsync(typeof(BroadcastAggregate));
        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
