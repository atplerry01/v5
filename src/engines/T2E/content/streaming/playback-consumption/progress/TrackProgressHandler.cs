using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Progress;

public sealed class TrackProgressHandler : IEngine
{
    private readonly IClock _clock;

    public TrackProgressHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TrackProgressCommand cmd) return Task.CompletedTask;
        var aggregate = ProgressAggregate.Track(
            new ProgressId(cmd.ProgressId),
            new SessionRef(cmd.SessionId),
            new PlaybackPosition(cmd.PositionMs),
            new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
