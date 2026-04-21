using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Availability;

public sealed class EnablePlaybackHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EnablePlaybackCommand cmd) return;
        var aggregate = (PlaybackAggregate)await context.LoadAggregateAsync(typeof(PlaybackAggregate));
        aggregate.Enable(new Timestamp(cmd.EnabledAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
