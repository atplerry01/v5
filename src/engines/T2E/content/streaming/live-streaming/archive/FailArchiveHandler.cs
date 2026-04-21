using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Archive;

public sealed class FailArchiveHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailArchiveCommand cmd) return;
        var aggregate = (ArchiveAggregate)await context.LoadAggregateAsync(typeof(ArchiveAggregate));
        aggregate.Fail(new ArchiveFailureReason(cmd.Reason), new Timestamp(cmd.FailedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
