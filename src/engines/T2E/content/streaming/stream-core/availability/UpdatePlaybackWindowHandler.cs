using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Availability;

public sealed class UpdatePlaybackWindowHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdatePlaybackWindowCommand cmd) return;
        var aggregate = (PlaybackAggregate)await context.LoadAggregateAsync(typeof(PlaybackAggregate));
        aggregate.UpdateWindow(
            new PlaybackWindow(new Timestamp(cmd.AvailableFrom), new Timestamp(cmd.AvailableUntil)),
            new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
