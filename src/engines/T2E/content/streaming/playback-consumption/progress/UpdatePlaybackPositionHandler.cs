using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Progress;

public sealed class UpdatePlaybackPositionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdatePlaybackPositionCommand cmd) return;
        var aggregate = (ProgressAggregate)await context.LoadAggregateAsync(typeof(ProgressAggregate));
        aggregate.UpdatePosition(new PlaybackPosition(cmd.PositionMs), new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
