using Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Content.Streaming.LiveStreaming.IngestSession;

public sealed class AuthenticateIngestSessionHandler : IEngine
{
    private readonly IClock _clock;

    public AuthenticateIngestSessionHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AuthenticateIngestSessionCommand cmd) return Task.CompletedTask;
        var aggregate = IngestSessionAggregate.Authenticate(
            new IngestSessionId(cmd.SessionId),
            new BroadcastRef(cmd.BroadcastId),
            new IngestEndpoint(cmd.Endpoint),
            new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
