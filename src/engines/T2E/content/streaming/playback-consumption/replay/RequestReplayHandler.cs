using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Replay;

public sealed class RequestReplayHandler : IEngine
{
    private readonly IClock _clock;

    public RequestReplayHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestReplayCommand cmd) return Task.CompletedTask;
        var aggregate = ReplayAggregate.Request(
            new ReplayId(cmd.ReplayId),
            new ArchiveRef(cmd.ArchiveId),
            new ViewerRef(cmd.ViewerId),
            new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
