using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.IngestSession;

public sealed class ResumeIngestSessionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeIngestSessionCommand cmd) return;
        var aggregate = (IngestSessionAggregate)await context.LoadAggregateAsync(typeof(IngestSessionAggregate));
        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
