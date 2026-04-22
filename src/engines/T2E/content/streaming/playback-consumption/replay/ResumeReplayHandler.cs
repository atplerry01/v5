using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Replay;

public sealed class ResumeReplayHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeReplayCommand cmd) return;
        var aggregate = (ReplayAggregate)await context.LoadAggregateAsync(typeof(ReplayAggregate));
        aggregate.Resume(new Timestamp(cmd.ResumedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
