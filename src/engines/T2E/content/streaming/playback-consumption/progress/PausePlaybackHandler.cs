using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Progress;

public sealed class PausePlaybackHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PausePlaybackCommand cmd) return;
        var aggregate = (ProgressAggregate)await context.LoadAggregateAsync(typeof(ProgressAggregate));
        aggregate.Pause(new PlaybackPosition(cmd.PositionMs), new Timestamp(cmd.PausedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
