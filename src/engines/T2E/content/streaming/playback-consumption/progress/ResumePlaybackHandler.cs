using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Progress;

public sealed class ResumePlaybackHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumePlaybackCommand cmd) return;
        var aggregate = (ProgressAggregate)await context.LoadAggregateAsync(typeof(ProgressAggregate));
        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
