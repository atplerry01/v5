using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.IngestSession;

public sealed class StallIngestSessionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StallIngestSessionCommand cmd) return;
        var aggregate = (IngestSessionAggregate)await context.LoadAggregateAsync(typeof(IngestSessionAggregate));
        aggregate.Stall(new Timestamp(cmd.StalledAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
