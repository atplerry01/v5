using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.StreamSession;

public sealed record StreamSessionOpenedEvent(
    StreamSessionId SessionId,
    StreamRef StreamRef,
    SessionWindow Window,
    Timestamp OpenedAt) : DomainEvent;
