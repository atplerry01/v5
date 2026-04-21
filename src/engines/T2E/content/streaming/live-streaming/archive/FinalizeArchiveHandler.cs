using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Archive;

public sealed class FinalizeArchiveHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FinalizeArchiveCommand cmd) return;
        var aggregate = (ArchiveAggregate)await context.LoadAggregateAsync(typeof(ArchiveAggregate));
        aggregate.Finalize(new Timestamp(cmd.FinalizedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
