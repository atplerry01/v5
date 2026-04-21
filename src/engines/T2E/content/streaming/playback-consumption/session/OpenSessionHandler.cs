using Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.PlaybackConsumption.Session;

public sealed class OpenSessionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not OpenSessionCommand cmd) return Task.CompletedTask;
        var aggregate = SessionAggregate.Open(
            new SessionId(cmd.SessionId),
            new StreamRef(cmd.StreamId),
            new SessionWindow(new Timestamp(cmd.OpenedAt), new Timestamp(cmd.ExpiresAt)),
            new Timestamp(cmd.OpenedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
