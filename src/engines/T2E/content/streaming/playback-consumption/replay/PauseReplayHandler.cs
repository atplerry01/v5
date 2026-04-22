using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Replay;

public sealed class PauseReplayHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PauseReplayCommand cmd) return;
        var aggregate = (ReplayAggregate)await context.LoadAggregateAsync(typeof(ReplayAggregate));
        aggregate.Pause(new PlaybackPosition(cmd.PositionMs), new Timestamp(cmd.PausedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
