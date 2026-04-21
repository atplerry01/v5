using Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.CoreObject.Transcript;

public sealed class ArchiveTranscriptHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveTranscriptCommand cmd) return;
        var aggregate = (TranscriptAggregate)await context.LoadAggregateAsync(typeof(TranscriptAggregate));
        aggregate.Archive(new Timestamp(cmd.ArchivedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
