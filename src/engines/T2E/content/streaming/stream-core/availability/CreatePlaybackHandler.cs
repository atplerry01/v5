using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Availability;

public sealed class CreatePlaybackHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreatePlaybackCommand cmd) return Task.CompletedTask;
        var aggregate = PlaybackAggregate.Create(
            new PlaybackId(cmd.PlaybackId),
            new PlaybackSourceRef(cmd.SourceId, Enum.Parse<PlaybackSourceKind>(cmd.SourceKind)),
            Enum.Parse<PlaybackMode>(cmd.Mode),
            new PlaybackWindow(new Timestamp(cmd.AvailableFrom), new Timestamp(cmd.AvailableUntil)),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
