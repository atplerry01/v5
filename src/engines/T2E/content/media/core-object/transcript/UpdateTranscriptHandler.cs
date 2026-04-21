using Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.CoreObject.Transcript;

public sealed class UpdateTranscriptHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateTranscriptCommand cmd) return;
        var aggregate = (TranscriptAggregate)await context.LoadAggregateAsync(typeof(TranscriptAggregate));
        aggregate.Update(new TranscriptOutputRef(cmd.OutputRef), new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
