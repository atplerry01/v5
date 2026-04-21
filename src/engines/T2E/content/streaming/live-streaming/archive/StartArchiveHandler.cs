using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.Archive;

public sealed class StartArchiveHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartArchiveCommand cmd) return Task.CompletedTask;
        StreamSessionRef? sessionRef = cmd.SessionId is { } sid ? new StreamSessionRef(sid) : null;
        var aggregate = ArchiveAggregate.Start(
            new ArchiveId(cmd.ArchiveId),
            new StreamRef(cmd.StreamId),
            sessionRef,
            new Timestamp(cmd.StartedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
